// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents operation executor.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface IOperationExecutor<TSessionState, TOperationState>
    {
        /// <summary>
        /// Executes operation and returns updated operation.
        /// </summary>
        /// <param name="session">Owner session.</param>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>Updated operation.</returns>
        Task<IOperation<TOperationState>> ExecuteAsync(
            ISession<TSessionState> session,
            IOperation<TOperationState> operation,
            CancellationToken cancellation);
    }

    /// <summary>
    /// Operation executor implemented with external function.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class OperationExecutor<TSessionState, TOperationState> : IOperationExecutor<TSessionState, TOperationState>
    {
        private readonly Func<ISession<TSessionState>, IOperation<TOperationState>, CancellationToken, Task<IOperation<TOperationState>>> _execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationExecutor{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="execute">Execute function.</param>
        public OperationExecutor(Func<ISession<TSessionState>, IOperation<TOperationState>, CancellationToken, Task<IOperation<TOperationState>>> execute)
        {
            execute.AssertArgumentNotNull(nameof(execute));

            _execute = execute;
        }

        /// <inheritdoc />
        public Task<IOperation<TOperationState>> ExecuteAsync(ISession<TSessionState> session, IOperation<TOperationState> operation, CancellationToken cancellation)
        {
            return _execute(session, operation, cancellation);
        }
    }
}
