// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// <see cref="ISessionManager"/> and <see cref="IOperationManager"/> builder.
    /// </summary>
    public class SessionBuilder<TSessionState, TOperationState>
    {
        private ILoggerFactory _loggerFactory;
        private ISessionStorage<TSessionState, TOperationState> _sessionStorage;
        private SessionManagerConfiguration _sessionManagerConfiguration;
        private ExecutionOptions<TSessionState, TOperationState> _executionOptions;

        public SessionBuilder(
            ILoggerFactory? loggerFactory = null,
            ISessionStorage<TSessionState, TOperationState>? sessionStorage = null)
        {
            _loggerFactory = loggerFactory ?? new NullLoggerFactory();
            _sessionStorage = sessionStorage ?? new ConcurrentDictionaryStorage<TSessionState, TOperationState>();
            _sessionManagerConfiguration = SessionManagerConfiguration.New();
            _executionOptions = new ExecutionOptions<TSessionState, TOperationState>();
        }

        public SessionBuilder<TSessionState, TOperationState> WithLogging(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            return this;
        }

        public SessionBuilder<TSessionState, TOperationState> WithStorage(ISessionStorage<TSessionState, TOperationState> sessionStorage)
        {
            _sessionStorage = sessionStorage;
            return this;
        }

        public SessionBuilder<TSessionState, TOperationState> WithMemoryCacheStorage(IMemoryCache memoryCache)
        {
            _sessionStorage = MemoryCacheStorage<TSessionState, TOperationState>.CreateFromSettings(memoryCache, _sessionManagerConfiguration);
            return this;
        }

        public SessionBuilder<TSessionState, TOperationState> ConfigureSessionManager(Action<SessionManagerConfiguration> configure)
        {
            configure(_sessionManagerConfiguration);
            return this;
        }

        public SessionBuilder<TSessionState, TOperationState> ConfigureExecution(Action<ExecutionOptions<TSessionState, TOperationState>> configure)
        {
            configure(_executionOptions);
            return this;
        }

        public ISessionManager<TSessionState, TOperationState> BuildSessionManager()
        {
            return new SessionManager<TSessionState, TOperationState>(
                configuration: _sessionManagerConfiguration,
                loggerFactory: _loggerFactory,
                sessionStorage: _sessionStorage);
        }

        public IOperationManager<TSessionState, TOperationState> CreateOperationManager(
            OperationId sessionId,
            TSessionState sessionState,
            IPropertyContainer? operationManagerMetadata = null)
        {
            ISessionManager<TSessionState, TOperationState> sessionManager = BuildSessionManager();

            IOperationManager<TSessionState, TOperationState> operationManager = new OperationManager<TSessionState, TOperationState>(
                sessionId: sessionId,
                sessionState: sessionState,
                sessionManager: sessionManager,
                executionOptions: _executionOptions,
                logger: null,
                metadata: operationManagerMetadata);

            sessionManager.AddOperationManager(operationManager);

            return operationManager;
        }
    }
}
