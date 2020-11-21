// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MicroElements.Processing.TaskManager.Metrics;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Operation execution context.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class OperationExecutionContext<TSessionState, TOperationState>
    {
        /// <summary>
        /// Gets current session state.
        /// </summary>
        public ISession<TSessionState> Session { get; }

        /// <summary>
        /// Gets current state of operation.
        /// </summary>
        public IOperation<TOperationState> Operation { get; }

        /// <summary>
        /// Gets cancellation token.
        /// </summary>
        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Gets a service to record operation timings.
        /// </summary>
        public ITracer Tracer { get; }

        /// <summary>
        /// Gets or sets new state for operation.
        /// </summary>
        [MaybeNull]
        [AllowNull]
        public TOperationState NewState { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationExecutionContext{TSessionState,TOperationState}"/> class.
        /// </summary>
        /// <param name="session">Owner session.</param>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <param name="tracer">Operation metrics.</param>
        public OperationExecutionContext(
            ISession<TSessionState> session,
            IOperation<TOperationState> operation,
            CancellationToken cancellation,
            ITracer tracer)
        {
            Session = session;
            Operation = operation;
            Cancellation = cancellation;
            Tracer = tracer;
            NewState = default;
        }
    }
}
