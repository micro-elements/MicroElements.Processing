// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using MicroElements.Functional;

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

    /// <summary>
    /// Operation executor implemented with external function.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class OperationExecutorExtended<TSessionState, TOperationState> : IOperationExecutorExtended<TSessionState, TOperationState>
    {
        private readonly Func<OperationExecutionContext<TSessionState, TOperationState>, Task> _execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationExecutorExtended{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="execute">Execute function.</param>
        public OperationExecutorExtended(Func<OperationExecutionContext<TSessionState, TOperationState>, Task> execute)
        {
            execute.AssertArgumentNotNull(nameof(execute));

            _execute = execute;
        }

        /// <summary>
        /// Executes operation and returns updated operation state.
        /// </summary>
        /// <param name="context">Operation execution context.</param>
        /// <returns>Task.</returns>
        public Task ExecuteAsync(OperationExecutionContext<TSessionState, TOperationState> context)
        {
            return _execute(context);
        }
    }
}
