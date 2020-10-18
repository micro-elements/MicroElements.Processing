using System;

namespace MicroElements.Processing.TaskManager
{
    internal interface ISessionFactory<TSessionState, TOperationState>
    {
        IOperationManager<TSessionState, TOperationState> Create(
            ISessionManager<TSessionState, TOperationState> sessionManager,
            string sessionId,
            TSessionState sessionState);
    }
}
