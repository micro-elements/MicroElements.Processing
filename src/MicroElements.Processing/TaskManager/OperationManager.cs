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
using MicroElements.Processing.Pipelines;
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
        private readonly IPropertyContainer _metadata;
        private readonly ILogger<OperationManager<TSessionState, TOperationState>> _logger;
        private readonly ConcurrentDictionary<OperationId, IOperation<TOperationState>> _operations;

        // Mutates on status change
        private ISession<TSessionState> _session;

        // Initializes on StartAll
        private IExecutionOptions<TSessionState, TOperationState> _options;
        private CancellationTokenSource _cts;
        private Pipeline<IOperation<TOperationState>>? _pipeline;
        private Task<ISession<TSessionState, TOperationState>>? _sessionCompletionTask;

        //TODO: metrics
        //TODO: progress
        //TODO: push model
        //TODO: retry

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationManager{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="sessionId">Session id.</param>
        /// <param name="sessionState">Initial session state.</param>
        /// <param name="sessionManager">Owner session manager.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public OperationManager(
            OperationId sessionId,
            TSessionState sessionState,
            ISessionManager<TSessionState, TOperationState> sessionManager,
            ILoggerFactory? loggerFactory = null)
        {
            _sessionManager = sessionManager;
            loggerFactory ??= (ILoggerFactory)_sessionManager.Services.GetService(typeof(ILoggerFactory)) ?? NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<OperationManager<TSessionState, TOperationState>>();
            _metadata = new MutablePropertyContainer();
            _operations = new ConcurrentDictionary<OperationId, IOperation<TOperationState>>();

            _session = TaskManager.Session.Create(
                id: sessionId,
                status: OperationStatus.NotStarted,
                state: sessionState);
        }

        /// <inheritdoc />
        public IPropertyContainer Metadata => _metadata;

        /// <inheritdoc />
        public ISessionManager SessionManager => _sessionManager;

        /// <inheritdoc />
        public ISession SessionUntyped => _session;

        /// <inheritdoc />
        public ISession<TSessionState, TOperationState> Session => _session.WithOperations(GetOperations());

        /// <inheritdoc />
        public Task<ISession<TSessionState, TOperationState>> SessionCompletion => _sessionCompletionTask ?? throw new OperationManagerException($"Session {_session.Id} is not started.");

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
        public IOperation<TOperationState> CreateOperation(OperationId operationId, TOperationState state)
        {
            var operation = Operation<TOperationState>.Empty.With(id: operationId, state: state);
            _operations[operationId] = operation;
            return operation;
        }

        /// <inheritdoc />
        public IOperation<TOperationState> UpdateOperation(OperationId operationId, Func<IOperation<TOperationState>, IOperation<TOperationState>> updateState)
        {
            IOperation<TOperationState>? operation = GetOperationOrThrow(operationId);

            var updated = updateState(operation);

            return UpdateOperation(operationId, updated);
        }

        /// <inheritdoc />
        public IOperation<TOperationState> UpdateOperation(OperationId operationId, IOperation<TOperationState> updatedOperation)
        {
            IOperation<TOperationState> operation = GetOperationOrThrow(operationId);

            if (!ReferenceEquals(updatedOperation, operation))
            {
                _operations.TryUpdate(operationId, updatedOperation, operation);
            }

            return updatedOperation;
        }

        /// <inheritdoc />
        public Task StartAll(IExecutionOptions<TSessionState, TOperationState> options)
        {
            if (_options != null)
                throw new OperationManagerException($"Session {_session.Id} already started");
            options.Executor.AssertArgumentNotNull(nameof(options.Executor));

            _options = options;
            _cts = CreateCancellation(_options);

            _pipeline = new Pipeline<IOperation<TOperationState>>()
                .AddStep(ProcessOperation, settings =>
                {
                    settings.MaxDegreeOfParallelism = _options.MaxConcurrencyLevel;
                    settings.ExecutionOptions.CancellationToken = _cts.Token;
                })
                .AddStep(operation => OnOperationFinished(operation));

            _session = _session.With(status: OperationStatus.InProgress, startedAt: DateTime.Now.ToLocalDateTime());
            _logger.LogInformation($"Session started. SessionId: {_session.Id}.");

            // Add operations to pipeline
            var operations = GetOperations();
            _pipeline.Input.PostMany(operations);

            // do not awaiting pipeline finished
            _sessionCompletionTask = _pipeline
                .CompleteAndWait()
                .ContinueWith(OnSessionFinished, TaskContinuationOptions.ExecuteSynchronously);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAll()
        {
            _cts?.Cancel();
            return Task.CompletedTask;
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
                throw new OperationManagerException($"Operation {operationId} is not exists.");
            }

            return operation;
        }

        private async Task<IOperation<TOperationState>> ProcessOperation(IOperation<TOperationState> operation)
        {
            // Set InProgress
            operation = UpdateOperation(operation.Id, operation
                .With(startedAt: DateTime.Now.ToLocalDateTime(), status: OperationStatus.InProgress));

            Stopwatch stopwatch = Stopwatch.StartNew();
            _logger.LogInformation($"Operation started.  Id: {operation.Id}.");

            IOperation<TOperationState> resultOperation;
            try
            {
                // Limit by global lock
                await _sessionManager.GlobalLock.WaitAsync();

                //TODO: Cancellation on operation level?
                //_cts.Token.ThrowIfCancellationRequested();

                // Run action
                resultOperation = await _options.Executor.ExecuteAsync(_session, operation, _cts.Token);
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
            resultOperation = UpdateOperation(operation.Id, resultOperation
                .With(finishedAt: DateTime.Now.ToLocalDateTime(), status: OperationStatus.Finished));

            _logger.LogInformation($"Operation finished. Id: {operation.Id}. Elapsed: {stopwatch.Elapsed}.");

            return resultOperation;
        }

        private void OnOperationFinished(IOperation<TOperationState> operation)
        {
            _options.OnOperationFinished?.Invoke(operation);
        }

        private ISession<TSessionState, TOperationState> OnSessionFinished(Task task)
        {
            _session = _session.With(status: OperationStatus.Finished, finishedAt: DateTime.Now.ToLocalDateTime());

            _logger.LogInformation($"Session finished. SessionId: {_session.Id}. Elapsed: {_session.GetDuration()}.");

            var sessionWithOperations = Session;

            if (_options.OnSessionFinished != null)
            {
                try
                {
                    _options.OnSessionFinished(sessionWithOperations);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in OnFinished callback.");
                }
            }

            return sessionWithOperations;
        }
    }
}
