// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents operation executor.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface IOperationExecutorExtended<TSessionState, TOperationState>
    {
        /// <summary>
        /// Executes operation and returns updated operation state.
        /// </summary>
        /// <param name="context">Operation execution context.</param>
        /// <returns>Task.</returns>
        Task ExecuteAsync(OperationExecutionContext<TSessionState, TOperationState> context);
    }
}
