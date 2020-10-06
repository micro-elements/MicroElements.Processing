// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents operation manager.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface IOperationManager<TSessionState, TOperationState>
    {
        /// <summary>
        /// Gets internal state as session.
        /// </summary>
        ISession<TSessionState, TOperationState> Session { get; }

        /// <summary>
        /// Gets session completion awaitable task.
        /// </summary>
        Task<ISession<TSessionState, TOperationState>> SessionCompletion { get; }

        /// <summary>
        /// Gets operation list.
        /// </summary>
        /// <returns>Operation list.</returns>
        IReadOnlyCollection<IOperation<TOperationState>> GetOperations();

        /// <summary>
        /// Gets operation by its id.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <returns>Operation if exists.</returns>
        IOperation<TOperationState>? GetOperation(OperationId operationId);

        /// <summary>
        /// Creates operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="state">Initial operation state.</param>
        /// <returns>Created operation.</returns>
        IOperation<TOperationState> CreateOperation(OperationId operationId, TOperationState state);

        /// <summary>
        /// Updates operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="updateState">Update function.</param>
        /// <returns>Updated operation.</returns>
        IOperation<TOperationState> UpdateOperation(OperationId operationId, Func<IOperation<TOperationState>, IOperation<TOperationState>> updateState);

        /// <summary>
        /// Updates operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="updatedOperation">Operation.</param>
        /// <returns>Updated operation.</returns>
        IOperation<TOperationState> UpdateOperation(OperationId operationId, IOperation<TOperationState> updatedOperation);

        /// <summary>
        /// Starts all operations (does not waits completion).
        /// </summary>
        /// <param name="options">Execution options.</param>
        /// <returns>Task.</returns>
        Task StartAll(ExecutionOptions<TSessionState, TOperationState> options);

        /// <summary>
        /// Stops session.
        /// </summary>
        /// <returns>Task.</returns>
        Task StopAll();
    }
}
