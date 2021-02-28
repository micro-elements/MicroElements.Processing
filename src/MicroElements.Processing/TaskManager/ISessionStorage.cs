// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents session storage.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface ISessionStorage<TSessionState, TOperationState>
    {
        /// <summary>
        /// Adds <paramref name="operationManager"/> to storage.
        /// </summary>
        /// <param name="sessionId">Session id.</param>
        /// <param name="operationManager">Operation manager to add.</param>
        void Set(string sessionId, IOperationManager<TSessionState, TOperationState> operationManager);

        /// <summary>
        /// Gets <see cref="IOperationManager{TSessionState,TOperationState}"/> from storage.
        /// </summary>
        /// <param name="sessionId">Session id.</param>
        /// <returns>Optional operation manager.</returns>
        IOperationManager<TSessionState, TOperationState>? Get(string sessionId);

        /// <summary>
        /// Gets all session keys.
        /// </summary>
        /// <returns>Session keys.</returns>
        IReadOnlyCollection<string> GetKeys();

        /// <summary>
        /// Deletes by key.
        /// </summary>
        /// <param name="sessionId">Session id.</param>
        void Delete(string sessionId);
    }
}
