// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        // Initializes in ctor
        private readonly ISessionManager _sessionManager;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<OperationId, IOperation<TOperationState>> _operations;

        // Replaces on status change
        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1);
        private ISession<TSessionState> _session;

        // Initializes on StartAll
        private IExecutionOptions<TSessionState, TOperationState>? _options;
        private CancellationTokenSource? _cts;
        private Pipeline<IOperation<TOperationState>>? _pipeline;
        private Task<ISession<TSessionState, TOperationState>>? _sessionCompletionTask;
        private SessionTracer _sessionTracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationManager{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="sessionId">Session id.</param>
        /// <param name="sessionState">Initial session state.</param>
        /// <param name="sessionManager">Owner session manager.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="metadata">Optional metadata.</param>
        public OperationManager(
            OperationId sessionId,
            TSessionState sessionState,
            ISessionManager<TSessionState, TOperationState> sessionManager,
            ILogger? logger = null,
            IPropertyContainer? metadata = null)
        {
            _sessionManager = sessionManager.AssertArgumentNotNull(nameof(sessionManager));
            _logger = logger ?? GetLoggerFactory().CreateLogger(sessionId.Value);
            _operations = new ConcurrentDictionary<OperationId, IOperation<TOperationState>>();

            _session = Operation
                .CreateNotStarted(sessionId, sessionState, metadata)
                .ToSession(getOperations: GetOperations);

            ILoggerFactory GetLoggerFactory() =>
                (ILoggerFactory)_sessionManager.Services.GetService(typeof(ILoggerFactory)) ?? NullLoggerFactory.Instance;
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
                _session = _session.With(state: newState);
            if (newMetadata.IsNotNull() && !ReferenceEquals(_session.State, newMetadata))
                _session = _session.With(metadata: newMetadata);

            return Session;
        }

        /// <inheritdoc />
        public ISession<TSessionState, TOperationState> SessionWithOperations => _session.WithOperations(GetOperations());

        /// <inheritdoc />
        public Task<ISession<TSessionState, TOperationState>> SessionCompletion => _sessionCompletionTask ?? throw new OperationManagerException(Errors.SessionIsNotStarted(_session.Id));

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
        public IOperation<TOperationState> CreateOperation(OperationId operationId, TOperationState state, IPropertyContainer? metadata = null)
        {
            var operation = Operation.CreateNotStarted(id: operationId, state: state, metadata: metadata);
            _operations[operationId] = operation;
            return operation;
        }

        /// <inheritdoc />
        public IOperation<TOperationState> UpdateOperation(OperationId operationId, IOperation<TOperationState> updatedOperation)
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
        public IOperation<TOperationState> UpdateOperation(OperationId operationId, Action<OperationUpdateContext<TOperationState>> updateAction)
        {
            updateAction.AssertArgumentNotNull(nameof(updateAction));

            IOperation<TOperationState> operation = GetOperationOrThrow(operationId);
            IOperation<TOperationState> updatedOperation = operation;

            var updateContext = new OperationUpdateContext<TOperationState>(operation);
            updateAction(updateContext);
            if (updateContext.NewState.IsNotNull() && !ReferenceEquals(updateContext.Operation.State, updateContext.NewState))
            {
                updatedOperation = operation.With(state: updateContext.NewState);
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
        public async Task Start(IExecutionOptions<TSessionState, TOperationState> options)
        {
            if (_options != null)
                throw new OperationManagerException(Errors.SessionIsAlreadyStarted(_session.Id));

            if (options.ExecutorExtended == null && options.Executor == null)
                throw new ArgumentException($"ExecutionOptions: {nameof(options.ExecutorExtended)} or {nameof(options.Executor)} should be provided.", nameof(options));
            if (options.ExecutorExtended != null && options.Executor != null)
                throw new ArgumentException($"ExecutionOptions: Only one of {nameof(options.ExecutorExtended)}, {nameof(options.Executor)} should be provided.", nameof(options));

            using var updateLock = await _updateLock.WaitAsyncAndGetLockReleaser();

            _options = options;
            _cts = CreateCancellation(_options);

            _pipeline = new Pipeline<IOperation<TOperationState>>()
                .AddStep(ProcessOperation, settings =>
                {
                    settings.MaxDegreeOfParallelism = _options.MaxConcurrencyLevel;
                    settings.ExecutionOptions.CancellationToken = _cts.Token;
                })
                .AddStep(operation => OnOperationFinished(operation));

            _session = _session.With(
                status: OperationStatus.InProgress,
                startedAt: DateTime.Now.ToLocalDateTime(),
                executionOptions: options);

            var sessionTags = new[] { new KeyValuePair<string, object?>("SessionId", _session.Id.ToString()), };
            _sessionTracer = new SessionTracer(_logger, sessionTags);

            _logger.LogInformation($"Session started. SessionId: {_session.Id}.");

            // Add operations to pipeline
            var operations = GetOperations();
            _pipeline.Input.PostMany(operations);

            // do not awaiting pipeline finished
            _sessionCompletionTask = _pipeline
                .CompleteAndWait()
                .ContinueWith(OnSessionFinished, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <inheritdoc />
        public void Stop()
        {
            _cts?.Cancel();
        }

        private static CancellationTokenSource CreateCancellation(IExecutionOptions<TSessionState, TOperationState> options)
        {
            var timeoutTokenSource = new CancellationTokenSource(options.SessionTimeout);
            var externalCancellationToken = options.CancellationToken;
            if (externalCancellationToken != default)
                return CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, externalCancellationToken);
            else
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
                var operationTags = new[] { new KeyValuePair<string, object?>("OperationId", operation.Id.ToString()) };
                using var operationTracer = new ChildTracer(_sessionTracer, "Operation", operationTags);

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
                        await _sessionManager.GlobalLock.WaitAsync(_cts!.Token);
                    }

                    // Mark global wait finished
                    operation = operation.With(
                        metadata: new MutablePropertyContainer(operation.Metadata)
                            .WithValue(OperationMeta.GlobalWaitDuration, stopwatch.Elapsed));

                    // Run action
                    using var executionTracer = new ChildTracer(operationTracer, "Execution");
                    {
                        if (_options!.Executor != null)
                        {
                            resultOperation = await _options.Executor.ExecuteAsync(_session, operation, _cts.Token);
                        }
                        else if (_options.ExecutorExtended != null)
                        {
                            var context = new OperationExecutionContext<TSessionState, TOperationState>(_session, operation, _cts.Token, executionTracer);
                            await _options.ExecutorExtended.ExecuteAsync(context);
                            resultOperation = operation.With(state: context.NewState);
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
                    _options.OnOperationFinished?.Invoke(_session, operation);
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

            string reason = _cts.IsCancellationRequested ? "(cancelled)" : string.Empty;

            _logger.LogInformation($"Session finished {reason}. SessionId: {_session.Id}. Elapsed: {_session.GetDuration()}.");

            _sessionTracer.Dispose();

            var sessionWithOperations = SessionWithOperations;

            if (_options.OnSessionFinished != null)
            {
                try
                {
                    _options.OnSessionFinished(sessionWithOperations);
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
