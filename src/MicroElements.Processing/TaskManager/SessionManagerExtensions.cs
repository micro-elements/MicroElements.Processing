// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class SessionManagerExtensions
    {
        /// <summary>
        /// Gets <see cref="IOperationManager{TSessionState}"/> or throws if it was not found.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <typeparam name="TOperationState">Operation state.</typeparam>
        /// <param name="sessionManager">Source session manager.</param>
        /// <param name="sessionId">SessionId.</param>
        /// <returns>OperationManager or null.</returns>
        public static IOperationManager<TSessionState, TOperationState> GetOperationManagerOrThrow<TSessionState, TOperationState>(
            this ISessionManager<TSessionState, TOperationState> sessionManager, string sessionId)
        {
            return sessionManager.GetOperationManager(sessionId) ?? throw new SessionManagerException(Errors.SessionDoesNotExists(sessionId));
        }

        /// <summary>
        /// Gets <see cref="ISession{TSessionState}"/> or throws if it was not found.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <typeparam name="TOperationState">Operation state.</typeparam>
        /// <param name="sessionManager">Source session manager.</param>
        /// <param name="sessionId">SessionId.</param>
        /// <returns>OperationManager or null.</returns>
        public static ISession<TSessionState, TOperationState> GetSessionOrThrow<TSessionState, TOperationState>(
            this ISessionManager<TSessionState, TOperationState> sessionManager, string sessionId)
        {
            return sessionManager.GetOperationManagerOrThrow(sessionId).SessionWithOperations;
        }

        /// <summary>
        /// Tries to delete session by session id and returns optional <see cref="Error"/>.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <typeparam name="TOperationState">Operation state.</typeparam>
        /// <param name="sessionManager">Source session manager.</param>
        /// <param name="sessionId">SessionId.</param>
        /// <returns>Optional error.</returns>
        public static Error<ErrorCode>? TryDeleteSession<TSessionState, TOperationState>(this ISessionManager<TSessionState, TOperationState> sessionManager, string sessionId)
        {
            return Error.Try<ErrorCode>(() =>
            {
                sessionManager.DeleteSession(sessionId);
            });
        }
    }
}
