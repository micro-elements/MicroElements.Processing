// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Data.Caching;
using MicroElements.Functional;
using Microsoft.Extensions.Caching.Memory;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Implements simple in memory session storage based on <see cref="IMemoryCache"/>.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class MemoryCacheStorage<TSessionState, TOperationState> : ISessionStorage<TSessionState, TOperationState>
    {
        private readonly ICacheSection<IOperationManager<TSessionState, TOperationState>> _sessionsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheStorage{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="memoryCache">Memory cache.</param>
        public MemoryCacheStorage(
            ISessionManagerConfiguration configuration,
            IMemoryCache memoryCache)
        {
            _sessionsCache = new CacheManager(
                    memoryCache.AssertArgumentNotNull(nameof(memoryCache)),
                    cacheContext => cacheContext.CacheEntry.AbsoluteExpirationRelativeToNow = configuration.SessionCacheTimeToLive)
                .GetOrCreateSection($"SessionManager_{configuration.Id}", CacheSettings<IOperationManager<TSessionState, TOperationState>>.Default);
        }

        /// <inheritdoc />
        public void Set(IOperationManager<TSessionState, TOperationState> operationManager)
        {
            _sessionsCache.Set(operationManager.Session.Id.Value, operationManager);
        }

        /// <inheritdoc />
        public IOperationManager<TSessionState, TOperationState>? Get(string sessionId)
        {
            return _sessionsCache
                .Get(sessionId)
                .GetValueOrDefault(defaultValue: null);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetKeys()
        {
            return _sessionsCache.Keys;
        }
    }
}
