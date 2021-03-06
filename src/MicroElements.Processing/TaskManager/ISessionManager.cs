﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using MicroElements.Metadata;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// SessionManager base interface with no write operations.
    /// Manages many sessions in parallel.
    /// </summary>
    public interface ISessionManager : IManualMetadataProvider
    {
        /// <summary>
        /// Gets session manager configuration.
        /// </summary>
        ISessionManagerConfiguration Configuration { get; }

        /// <summary>
        /// Gets session manager global lock.
        /// </summary>
        SemaphoreSlim GlobalLock { get; }

        /// <summary>
        /// Gets services that can be used by operation managers.
        /// </summary>
        IServiceProvider Services { get; }
    }

    /// <summary>
    /// SessionManager manages many sessions in parallel.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface ISessionManager<TSessionState, TOperationState> : ISessionManager
    {
        /// <summary>
        /// Adds new operation manager.
        /// </summary>
        /// <param name="operationManager">OperationManager instance.</param>
        /// <returns>Added OperationManager.</returns>
        IOperationManager<TSessionState, TOperationState> AddOperationManager(IOperationManager<TSessionState, TOperationState> operationManager);

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
