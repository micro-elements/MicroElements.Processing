// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// SessionStorage based on <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class ConcurrentDictionaryStorage<TSessionState, TOperationState> : ISessionStorage<TSessionState, TOperationState>
    {
        private readonly ConcurrentDictionary<string, IOperationManager<TSessionState, TOperationState>> _sessions = new ();

        /// <inheritdoc />
        public void Set(string sessionId, IOperationManager<TSessionState, TOperationState> operationManager)
        {
            _sessions[sessionId] = operationManager;
        }

        /// <inheritdoc />
        public IOperationManager<TSessionState, TOperationState>? Get(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var operationManager);
            return operationManager;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetKeys()
        {
            ICollection<string> sessionsKeys = _sessions.Keys;
            return sessionsKeys is IReadOnlyCollection<string> readOnlyCollection ? readOnlyCollection : sessionsKeys.ToList();
        }

        /// <inheritdoc />
        public void Delete(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }
    }
}
