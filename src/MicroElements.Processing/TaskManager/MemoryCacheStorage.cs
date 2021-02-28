// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="cacheSectionName">Optional cache section name.</param>
        /// <param name="configureCacheEntry">Optional cache configure method for all sections.</param>
        public MemoryCacheStorage(
            IMemoryCache memoryCache,
            string? cacheSectionName = null,
            Action<ICacheEntryContext>? configureCacheEntry = null)
        {
            memoryCache.AssertArgumentNotNull(nameof(memoryCache));

            string sectionName = cacheSectionName ?? $"OperationManager<{nameof(TSessionState)}, {nameof(TOperationState)}>";

            _sessionsCache = new CacheManager(memoryCache, configureCacheEntry)
                .GetOrCreateSection(sectionName, CacheSettings<IOperationManager<TSessionState, TOperationState>>.Default);
        }

        /// <summary>
        /// Creates new storage filled from <see cref="ISessionManagerConfiguration"/>.
        /// </summary>
        /// <param name="memoryCache">Memory cache to use.</param>
        /// <param name="configuration">Configuration ro use.</param>
        /// <returns>MemoryCacheStorage instance.</returns>
        public static MemoryCacheStorage<TSessionState, TOperationState> CreateFromSettings(
            IMemoryCache memoryCache,
            ISessionManagerConfiguration configuration)
        {
            string sectionName = $"OperationManager{configuration.Id}";

            return new MemoryCacheStorage<TSessionState, TOperationState>(
                memoryCache: memoryCache,
                cacheSectionName: sectionName,
                configureCacheEntry: cacheContext =>
                {
                    cacheContext.CacheEntry.AbsoluteExpirationRelativeToNow = configuration.SessionCacheTimeToLive;
                });
        }

        /// <inheritdoc />
        public void Set(string sessionId, IOperationManager<TSessionState, TOperationState> operationManager)
        {
            _sessionsCache.Set(sessionId, operationManager);
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

        /// <inheritdoc />
        public void Delete(string sessionId)
        {
            _sessionsCache.Remove(sessionId);
        }
    }
}
