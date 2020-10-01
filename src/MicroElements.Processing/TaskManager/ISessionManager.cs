// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// SessionManager base interface with no write operations.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Gets session manager configuration.
        /// </summary>
        ISessionManagerConfiguration Configuration { get; }

        /// <summary>
        /// Gets session manager global lock.
        /// </summary>
        SemaphoreSlim GlobalLock { get; }
    }

    /// <summary>
    /// SessionManager manages many <see cref="IOperationManager{TSessionState,TOperationState}"/>.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface ISessionManager<TSessionState, TOperationState> : ISessionManager
    {
        /// <summary>
        /// Adds new operation manager.
        /// </summary>
        /// <param name="sessionId">SessionId.</param>
        /// <param name="operationManager">OperationManager instance.</param>
        void AddOperationManager(string sessionId, IOperationManager<TSessionState, TOperationState> operationManager);

        /// <summary>
        /// Gets OperationManager by its id.
        /// </summary>
        /// <param name="sessionId">SessionId.</param>
        /// <returns>OperationManager or null.</returns>
        IOperationManager<TSessionState, TOperationState>? GetOperationManager(string sessionId);

        /// <summary>
        /// Gets all sessions.
        /// </summary>
        /// <returns>Session list.</returns>
        IReadOnlyCollection<ISession<TSessionState, TOperationState>> GetSessions();

        /// <summary>
        /// Gets session by session id.
        /// </summary>
        /// <param name="sessionId">SessionId.</param>
        /// <returns>Session or null.</returns>
        ISession<TSessionState, TOperationState>? GetSession(string sessionId);

        /// <summary>
        /// Deletes session by session id.
        /// </summary>
        /// <param name="sessionId">SessionId.</param>
        void DeleteSession(string sessionId);
    }
}
