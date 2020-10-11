// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using MicroElements.Functional;
using Microsoft.Extensions.Logging;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Session manager manages many operation managers.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class SessionManager<TSessionState, TOperationState> : ISessionManager<TSessionState, TOperationState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="sessionStorage">Session storage.</param>
        /// <param name="initServices">Initializes <see cref="Services"/> that can be used in operation managers.</param>
        public SessionManager(
            ISessionManagerConfiguration configuration,
            ILoggerFactory loggerFactory,
            ISessionStorage<TSessionState, TOperationState>? sessionStorage = null,
            Action<IServiceContainer>? initServices = null)
        {
            Configuration = configuration.AssertArgumentNotNull(nameof(configuration));
            LoggerFactory = loggerFactory.AssertArgumentNotNull(nameof(loggerFactory));
            SessionStorage = sessionStorage.AssertArgumentNotNull(nameof(sessionStorage));

            GlobalLock = new SemaphoreSlim(configuration.MaxConcurrencyLevel);

            ServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ILoggerFactory), LoggerFactory);
            initServices?.Invoke(serviceContainer);
            Services = serviceContainer;
        }

        private ISessionStorage<TSessionState, TOperationState> SessionStorage { get; }

        private ILoggerFactory LoggerFactory { get; }

        /// <inheritdoc />
        public ISessionManagerConfiguration Configuration { get; }

        /// <inheritdoc />
        public SemaphoreSlim GlobalLock { get; }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public IOperationManager<TSessionState, TOperationState> AddOperationManager(IOperationManager<TSessionState, TOperationState> operationManager)
        {
            if (operationManager.SessionManager != this)
                throw new ArgumentException("OperationManager.SessionManager should be the same as target SessionManager", nameof(operationManager));

            SessionStorage.Set(operationManager);

            return operationManager;
        }

        /// <inheritdoc />
        public IOperationManager<TSessionState, TOperationState>? GetOperationManager(string sessionId)
        {
            return SessionStorage.Get(sessionId);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ISession<TSessionState, TOperationState>> GetSessions()
        {
            var sessions =
                SessionStorage
                    .GetKeys()
                    .Select(GetSession)
                    .Where(session => session != null)
                    .Cast<ISession<TSessionState, TOperationState>>()
                    .ToArray();

            return sessions;
        }

        /// <inheritdoc />
        public ISession<TSessionState, TOperationState>? GetSession(string sessionId)
        {
            return GetOperationManager(sessionId)?.SessionWithOperations;
        }

        /// <inheritdoc />
        public void DeleteSession(string sessionId)
        {
            GetOperationManager(sessionId)?.StopAll();
        }
    }
}
