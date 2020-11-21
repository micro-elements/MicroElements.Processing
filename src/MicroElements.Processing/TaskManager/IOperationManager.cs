// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MicroElements.Metadata;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents operation manager (base interface).
    /// Operation manager manages many operations in parallel.
    /// </summary>
    public interface IOperationManager : IMetadataProvider
    {
        /// <summary>
        /// Gets session manager that owns this operation manager.
        /// </summary>
        ISessionManager SessionManager { get; }

        /// <summary>
        /// Gets internal state as session.
        /// </summary>
        ISession SessionUntyped { get; }
    }

    /// <summary>
    /// Represents operation manager.
    /// Operation manager manages many operations in parallel.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    public interface IOperationManager<TSessionState> : IOperationManager
    {
        /// <summary>
        /// Gets internal state as session.
        /// </summary>
        ISession<TSessionState> Session { get; }

        /// <summary>
        /// Updates session.
        /// </summary>
        /// <param name="updateAction">Update action to provide new values.</param>
        /// <returns>Updated session.</returns>
        ISession<TSessionState> UpdateSession([DisallowNull] Action<SessionUpdateContext<TSessionState>> updateAction);
    }

    /// <summary>
    /// Represents operation manager.
    /// Operation manager manages many operations in parallel.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface IOperationManager<TSessionState, TOperationState> : IOperationManager<TSessionState>
    {
        /// <summary>
        /// Gets internal state as session including operations.
        /// </summary>
        ISession<TSessionState, TOperationState> SessionWithOperations { get; }

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
        /// <param name="metadata">Optional metadata.</param>
        /// <returns>Created operation.</returns>
        IOperation<TOperationState> CreateOperation(OperationId operationId, [DisallowNull] TOperationState state, IPropertyContainer? metadata = null);

        /// <summary>
        /// Updates operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="updatedOperation">Operation.</param>
        /// <returns>Updated operation.</returns>
        IOperation<TOperationState> UpdateOperation(OperationId operationId, [DisallowNull] IOperation<TOperationState> updatedOperation);

        /// <summary>
        /// Updates operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="updateAction">Update action.</param>
        /// <returns>Updated operation.</returns>
        IOperation<TOperationState> UpdateOperation(OperationId operationId, [DisallowNull] Action<OperationUpdateContext<TOperationState>> updateAction);

        /// <summary>
        /// Deletes operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <returns>Deleted operation if exists.</returns>
        IOperation<TOperationState>? DeleteOperation(OperationId operationId);

        /// <summary>
        /// Starts all operations (does not waits completion).
        /// </summary>
        /// <param name="options">Execution options.</param>
        /// <returns>Task.</returns>
        Task Start(IExecutionOptions<TSessionState, TOperationState> options);

        /// <summary>
        /// Stops session.
        /// </summary>
        void Stop();
    }
}
