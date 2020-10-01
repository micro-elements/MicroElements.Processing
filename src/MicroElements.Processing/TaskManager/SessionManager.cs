using System.Collections.Generic;
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
        private readonly ILoggerFactory _loggerProvider;
        private readonly ICacheSection<IOperationManager<TSessionState, TOperationState>> _sessionsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="loggerProvider">Logger factory.</param>
        /// <param name="memoryCache">Memory cache for session store.</param>
        public SessionManager(
            ISessionManagerConfiguration configuration,
            ILoggerFactory loggerProvider,
            IMemoryCache memoryCache)
        {
            Configuration = configuration.AssertArgumentNotNull(nameof(configuration));

            GlobalLock = new SemaphoreSlim(configuration.MaxConcurrencyLevel);

            _loggerProvider = loggerProvider.AssertArgumentNotNull(nameof(loggerProvider));

            _sessionsCache = new CacheManager(
                    memoryCache.AssertArgumentNotNull(nameof(memoryCache)),
                    cacheContext => cacheContext.CacheEntry.AbsoluteExpirationRelativeToNow = configuration.SessionCacheTimeToLive)
                .GetOrCreateSection($"SessionManager_{Configuration.Id}", CacheSettings<IOperationManager<TSessionState, TOperationState>>.Default);
        }

        /// <inheritdoc />
        public ISessionManagerConfiguration Configuration { get; }

        /// <inheritdoc />
        public SemaphoreSlim GlobalLock { get; }

        /// <inheritdoc />
        public void AddOperationManager(string sessionId, IOperationManager<TSessionState, TOperationState> operationManager)
        {
            _sessionsCache.Set(sessionId, operationManager);
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
