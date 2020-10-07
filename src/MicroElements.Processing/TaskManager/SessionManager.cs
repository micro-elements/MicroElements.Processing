using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using MicroElements.Data.Caching;
using MicroElements.Functional;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICacheSection<IOperationManager<TSessionState, TOperationState>> _sessionsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="memoryCache">Memory cache for session store.</param>
        public SessionManager(
            ISessionManagerConfiguration configuration,
            ILoggerFactory loggerFactory,
            IMemoryCache memoryCache)
        {
            Configuration = configuration.AssertArgumentNotNull(nameof(configuration));
            _loggerFactory = loggerFactory.AssertArgumentNotNull(nameof(loggerFactory));

            GlobalLock = new SemaphoreSlim(configuration.MaxConcurrencyLevel);

            _sessionsCache = new CacheManager(
                    memoryCache.AssertArgumentNotNull(nameof(memoryCache)),
                    cacheContext => cacheContext.CacheEntry.AbsoluteExpirationRelativeToNow = configuration.SessionCacheTimeToLive)
                .GetOrCreateSection($"SessionManager_{Configuration.Id}", CacheSettings<IOperationManager<TSessionState, TOperationState>>.Default);

            ServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ILoggerFactory), _loggerFactory);
            serviceContainer.AddService(typeof(IMemoryCache), memoryCache);
            Services = serviceContainer;
        }

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

            _sessionsCache.Set(operationManager.Session.Id.Value, operationManager);
            return operationManager;
        }

        /// <inheritdoc />
        public IOperationManager<TSessionState, TOperationState>? GetOperationManager(string sessionId)
        {
            return _sessionsCache
                .Get(sessionId)
                .GetValueOrDefault();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ISession<TSessionState, TOperationState>> GetSessions()
        {
            var sessions =
                _sessionsCache
                    .Keys
                    .Select(GetSession)
                    .Where(session => session != null)
                    .Cast<ISession<TSessionState, TOperationState>>()
                    .ToArray();

            return sessions;
        }

        /// <inheritdoc />
        public ISession<TSessionState, TOperationState>? GetSession(string sessionId)
        {
            return _sessionsCache
                .Get(sessionId)
                .Map(manager => manager.Session)
                .GetValueOrDefault();
        }

        /// <inheritdoc />
        public void DeleteSession(string sessionId)
        {
            GetOperationManager(sessionId)?.StopAll();
        }
    }
}
