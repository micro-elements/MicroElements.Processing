// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Processing.Common;
using MicroElements.Processing.Pipelines;
using MicroElements.Processing.TaskManager.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime.Extensions;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Default operation manager implementation.
    /// </summary>
    /// <typeparam name="TSessionState">Session state common to all operations.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class OperationManager<TSessionState, TOperationState> : IOperationManager<TSessionState, TOperationState>
    {
        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1);

        // Initializes in ctor
        private readonly ISessionManager _sessionManager;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<OperationId, IOperation<TOperationState>> _operations;

        // Initializes in Start
        private Runtime? _runtime;

        // Replaces on status change
        private ISession<TSessionState> _session;

        // Initializes in Start
        private class Runtime
        {
            public IExecutionOptions<TSessionState, TOperationState> Options { get; }

            public CancellationTokenSource Cts { get; }

            public Pipeline<IOperation<TOperationState>> Pipeline { get; }

            public SessionTracer SessionTracer { get; }

            public Task<ISession<TSessionState, TOperationState>> SessionCompletionTask { get; private set; } = null!;

            private readonly Func<Task, ISession<TSessionState, TOperationState>> _onSessionFinished;

            public Runtime(
                IExecutionOptions<TSessionState, TOperationState> options,
                CancellationTokenSource cts,
                Pipeline<IOperation<TOperationState>> pipeline,
                SessionTracer sessionTracer,
                Func<Task, ISession<TSessionState, TOperationState>> onSessionFinished)
            {
                Options = options;
                Cts = cts;
                Pipeline = pipeline;
                SessionTracer = sessionTracer;
                _onSessionFinished = onSessionFinished;
            }

            public void Start(IEnumerable<IOperation<TOperationState>> operations)
            {
                // Add operations to pipeline
                Pipeline.Input.PostMany(operations);

                // Complete pipeline and create completion task (not awaiting pipeline finished)
                if (Options.SessionType == SessionType.OperationsBatch)
                {
                    SessionCompletionTask = Pipeline
                        .CompleteAndWait()
                        .ContinueWith(_onSessionFinished, TaskContinuationOptions.ExecuteSynchronously);
                }

                if (Options.SessionType == SessionType.InfiniteProcess)
                {
                    SessionCompletionTask = Task.Delay(Options.SessionTimeout ?? TimeSpan.MaxValue)
                        .ContinueWith(_onSessionFinished, TaskContinuationOptions.ExecuteSynchronously);
                }
            }

            public void StopProcessing()
            {
                if (Options.SessionType == SessionType.InfiniteProcess)
                {
                    SessionCompletionTask = Pipeline
                        .CompleteAndWait()
                        .ContinueWith(_onSessionFinished, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationManager{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="sessionId">Session id.</param>
        /// <param name="sessionState">Initial session state.</param>
        /// <param name="sessionManager">Owner session manager.</param>
        /// <param name="executionOptions">Optional execution options. Also can be provided in Start method.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="metadata">Optional metadata.</param>
        public OperationManager(
            OperationId sessionId,
            [DisallowNull] TSessionState sessionState,
            ISessionManager<TSessionState, TOperationState> sessionManager,
            IExecutionOptions<TSessionState, TOperationState>? executionOptions = null,
            ILogger? logger = null,
            IPropertyContainer? metadata = null)
        {
            sessionState.AssertArgumentNotNull(nameof(sessionState));
            sessionManager.AssertArgumentNotNull(nameof(sessionManager));

            _sessionManager = sessionManager;
            _logger = logger ?? GetLoggerFactory().CreateLogger(sessionId.Value);
            _operations = new ConcurrentDictionary<OperationId, IOperation<TOperationState>>();

            _session = Operation
                .CreateNotStarted(sessionId, sessionState, metadata)
                .ToSession(getOperations: GetOperations);

            if (executionOptions != null)
                _session = _session.With(executionOptions: executionOptions);

            ILoggerFactory GetLoggerFactory() =>
                (ILoggerFactory?)_sessionManager.Services.GetService(typeof(ILoggerFactory)) ?? NullLoggerFactory.Instance;
        }

        /// <inheritdoc />
        public IPropertyContainer Metadata => _session.Metadata;

        /// <inheritdoc />
        public ISessionManager SessionManager => _sessionManager;

        /// <inheritdoc />
        ISession IOperationManager.SessionUntyped => _session;

        /// <inheritdoc />
        public ISession<TSessionState> Session => _session;

        /// <inheritdoc />
        public ISession<TSessionState> UpdateSession(Action<SessionUpdateContext<TSessionState>> updateAction)
        {
            updateAction.AssertArgumentNotNull(nameof(updateAction));

            using var updateLock = _updateLock.WaitAndGetLockReleaser();

            if (_session.Status != OperationStatus.NotStarted)
                throw new OperationManagerException(Errors.SessionUpdateIsProhibited(sessionId: _session.Id.Value, sessionStatus: _session.Status.ToString()));

            var updateContext = new SessionUpdateContext<TSessionState>(Session);
            updateAction(updateContext);

            var newState = updateContext.NewState;
            var newMetadata = updateContext.NewMetadata;

            if (newState.IsNotNull() && !ReferenceEquals(_session.State, newState))
                _session = _session.WithState(state: newState);
            if (newMetadata.IsNotNull() && !ReferenceEquals(_session.State, newMetadata))
                _session = _session.With(metadata: newMetadata);

            return Session;
        }

        /// <inheritdoc />
        public ISession<TSessionState, TOperationState> SessionWithOperations => _session.WithOperations(GetOperations());

        /// <inheritdoc />
        public Task<ISession<TSessionState, TOperationState>> SessionCompletion => GetRuntimeOrThrow().SessionCompletionTask;

        /// <inheritdoc />
        public IReadOnlyCollection<IOperation<TOperationState>> GetOperations()
        {
            return _operations.Values.ToArray();
        }

        /// <inheritdoc />
        public IOperation<TOperationState>? GetOperation(OperationId operationId)
        {
            _operations.TryGetValue(operationId, out IOperation<TOperationState> operation);
            return operation;
        }

        /// <inheritdoc />
        public IOperation<TOperationState> CreateOperation(OperationId operationId, [DisallowNull] TOperationState state, IPropertyContainer? metadata = null)
        {
            var operation = Operation.CreateNotStarted(id: operationId, state: state, metadata: metadata);
            _operations[operationId] = operation;
            return operation;
        }

        /// <inheritdoc />
        public IOperation<TOperationState> UpdateOperation(OperationId operationId, [DisallowNull] IOperation<TOperationState> updatedOperation)
        {
            updatedOperation.AssertArgumentNotNull(nameof(updatedOperation));
            IOperation<TOperationState> operation = GetOperationOrThrow(operationId);

            if (!updatedOperation.Id.Equals(operationId))
            {
                throw new OperationManagerException(Errors.OperationIdDoesNotMatch(
                    providedOperationId: updatedOperation.Id,
                    existingOperationId: operationId));
            }

            if (!ReferenceEquals(updatedOperation, operation))
            {
                _operations.TryUpdate(operationId, updatedOperation, operation);
            }

            return updatedOperation;
        }

        /// <inheritdoc />
        public IOperation<TOperationState> UpdateOperation(OperationId operationId, [DisallowNull] Action<OperationUpdateContext<TOperationState>> updateAction)
        {
            updateAction.AssertArgumentNotNull(nameof(updateAction));

            IOperation<TOperationState> operation = GetOperationOrThrow(operationId);
            IOperation<TOperationState> updatedOperation = operation;

            var updateContext = new OperationUpdateContext<TOperationState>(operation);
            updateAction(updateContext);
            if (updateContext.NewState.IsNotNull() && !ReferenceEquals(updateContext.Operation.State, updateContext.NewState))
            {
                updatedOperation = operation.WithState(state: updateContext.NewState);
                _operations.TryUpdate(operationId, updatedOperation, operation);
            }

            return updatedOperation;
        }

        /// <inheritdoc />
        public IOperation<TOperationState>? DeleteOperation(OperationId operationId)
        {
            _operations.TryRemove(operationId, out var deleted);
            return deleted;
        }

        /// <inheritdoc />
        public async Task Start(IExecutionOptions<TSessionState, TOperationState>? executionOptions = null)
        {
            if (_runtime != null)
                throw new OperationManagerException(Errors.SessionIsAlreadyStarted(_session.Id));

            if (_session.ExecutionOptions is IExecutionOptions<TSessionState, TOperationState> sessionExecutionOptions)
            {
                // Merge params provided on initialization and on start.
                executionOptions = new ExecutionOptions<TSessionState, TOperationState>()
                {
                    MaxConcurrencyLevel = executionOptions?.MaxConcurrencyLevel ?? sessionExecutionOptions.MaxConcurrencyLevel ?? Environment.ProcessorCount,
                    SessionTimeout = executionOptions?.SessionTimeout ?? sessionExecutionOptions.SessionTimeout ?? TimeSpan.FromHours(24),

                    Executor = executionOptions?.Executor ?? sessionExecutionOptions.Executor,
                    ExecutorExtended = executionOptions?.ExecutorExtended ?? sessionExecutionOptions.ExecutorExtended,
                    OnOperationFinished = executionOptions?.OnOperationFinished ?? sessionExecutionOptions.OnOperationFinished,
                    OnSessionFinished = executionOptions?.OnSessionFinished ?? sessionExecutionOptions.OnSessionFinished,

                    CancellationToken = executionOptions?.CancellationToken ?? CancellationToken.None,
                };
            }

            if (executionOptions == null)
                throw new ArgumentNullException(nameof(executionOptions), "Provide executionOptions on start or initialization.");
            if (executionOptions.ExecutorExtended == null && executionOptions.Executor == null)
                throw new ArgumentException($"ExecutionOptions: {nameof(executionOptions.ExecutorExtended)} or {nameof(executionOptions.Executor)} should be provided.", nameof(executionOptions));
            if (executionOptions.ExecutorExtended != null && executionOptions.Executor != null)
                throw new ArgumentException($"ExecutionOptions: Only one of {nameof(executionOptions.ExecutorExtended)}, {nameof(executionOptions.Executor)} should be provided.", nameof(executionOptions));

            using var updateLock = await _updateLock.WaitAsyncAndGetLockReleaser();

            _session = _session.With(
                status: OperationStatus.InProgress,
                startedAt: DateTime.Now.ToLocalDateTime(),
                executionOptions: executionOptions);

            var cts = CreateCancellation(executionOptions);

            var pipeline = new Pipeline<IOperation<TOperationState>>()
                .AddStep(operation => ProcessOperation(operation), settings =>
                {
                    settings.MaxDegreeOfParallelism = executionOptions.MaxConcurrencyLevel!.Value;
                    settings.ExecutionOptions.CancellationToken = cts.Token;
                })
                .AddStep(operation => OnOperationFinished(operation));

            var sessionTags = new[] { new KeyValuePair<string, object?>("SessionId", _session.Id.ToString()), };
            var sessionTracer = new SessionTracer(_logger, "Session", sessionTags);

            _runtime = new Runtime(
                executionOptions,
                cts,
                pipeline,
                sessionTracer,
                task => OnSessionFinished(task));

            // Start session
            _logger.LogInformation($"Session started. SessionId: {_session.Id}.");
            var operations = GetOperations();
            _runtime.Start(operations);
        }

        /// <inheritdoc />
        public void Stop()
        {
            _runtime?.Cts.Cancel();
        }

        private Runtime GetRuntimeOrThrow() => _runtime ?? throw new OperationManagerException(Errors.SessionIsNotStarted(_session.Id));

        private static CancellationTokenSource CreateCancellation(IExecutionOptions<TSessionState, TOperationState> options)
        {
            var timeoutTokenSource = new CancellationTokenSource(options.SessionTimeout!.Value);
            if (options.CancellationToken != default)
                return CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, options.CancellationToken);
            return timeoutTokenSource;
        }

        private IOperation<TOperationState> GetOperationOrThrow(OperationId operationId)
        {
            IOperation<TOperationState>? operation = GetOperation(operationId);
            if (operation == null)
            {
                throw new OperationManagerException(Errors.OperationDoesNotExists(operationId));
            }

            return operation;
        }

        private async Task<IOperation<TOperationState>> ProcessOperation(IOperation<TOperationState> operation)
        {
            IOperation<TOperationState> resultOperation = operation;
            try
            {
                var runtime = GetRuntimeOrThrow();

                var operationTags = new[] { new KeyValuePair<string, object?>("OperationId", operation.Id.ToString()) };
                using var operationTracer = new ChildTracer(runtime.SessionTracer, "Operation", operationTags);

                // Set InProgress
                operation = operation.With(
                    startedAt: DateTime.Now.ToLocalDateTime(),
                    status: OperationStatus.InProgress);
                operation = UpdateOperation(operation.Id, operation);

                Stopwatch stopwatch = Stopwatch.StartNew();
                _logger.LogInformation($"Operation started.  Id: {operation.Id}.");

                try
                {
                    using (operationTracer.StartActivity("WaitExecution"))
                    {
                        // Limit by global lock
                        await _sessionManager.GlobalLock.WaitAsync(runtime.Cts!.Token);
                    }

                    // Mark global wait finished
                    operation = operation.With(
                        metadata: new MutablePropertyContainer(operation.Metadata)
                            .WithValue(OperationMeta.GlobalWaitDuration, stopwatch.Elapsed));

                    // Run action
                    using var executionTracer = new ChildTracer(operationTracer, "Execution");
                    {
                        if (runtime.Options!.Executor != null)
                        {
                            resultOperation = await runtime.Options.Executor.ExecuteAsync(_session, operation, runtime.Cts.Token);
                        }
                        else if (runtime.Options.ExecutorExtended != null)
                        {
                            var context = new OperationExecutionContext<TSessionState, TOperationState>(_session, operation, runtime.Cts.Token, executionTracer);

                            await runtime.Options.ExecutorExtended.ExecuteAsync(context);

                            if (context.NewState != null && !ReferenceEquals(context.NewState, operation.State))
                                resultOperation = operation.WithState(state: context.NewState);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Set exception
                    resultOperation = operation.With(exception: e);
                }
                finally
                {
                    _sessionManager.GlobalLock.Release();
                }

                // Set Finished
                resultOperation = resultOperation.With(
                    finishedAt: DateTime.Now.ToLocalDateTime(),
                    status: OperationStatus.Finished);
                resultOperation = UpdateOperation(operation.Id, resultOperation);

                _logger.LogInformation($"Operation finished. Id: {operation.Id}. Elapsed: {stopwatch.Elapsed}.");
            }
            catch (OperationManagerException e)
            {
                var error = $"OperationManager {_session.Id} processing error: {e.Error}";
                _session.Messages.AddError(error);
                _logger.LogError(e, error);
            }

            return resultOperation;
        }

        private void OnOperationFinished(IOperation<TOperationState> operation)
        {
            try
            {
                if (operation.Status == OperationStatus.Finished)
                {
                    _runtime!.Options.OnOperationFinished?.Invoke(_session, operation);
                }
            }
            catch (Exception e)
            {
                _session.Messages.AddError($"Error in OnOperationFinished callback. Message: {e.Message}");
                _logger.LogError(e, "Error in OnOperationFinished callback.");
            }
        }

        private ISession<TSessionState, TOperationState> OnSessionFinished(Task task)
        {
            _session = _session.With(status: OperationStatus.Finished, finishedAt: DateTime.Now.ToLocalDateTime());
            var runtime = _runtime!;

            string reason = runtime.Cts.IsCancellationRequested ? "(cancelled)" : string.Empty;

            _logger.LogInformation($"Session finished {reason}. SessionId: {_session.Id}. Elapsed: {_session.GetDuration()}.");

            runtime.SessionTracer.Dispose();

            var sessionWithOperations = SessionWithOperations;

            if (runtime.Options.OnSessionFinished != null)
            {
                try
                {
                    runtime.Options.OnSessionFinished(sessionWithOperations);
                }
                catch (Exception e)
                {
                    _session.Messages.AddError($"Error in OnSessionFinished callback. Message: {e.Message}");
                    _logger.LogError(e, "Error in OnSessionFinished callback.");
                }
            }

            return sessionWithOperations;
        }
    }
}
