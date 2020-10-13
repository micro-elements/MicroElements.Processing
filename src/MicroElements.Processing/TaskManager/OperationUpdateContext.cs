// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Update context for <see cref="IOperation{TOperationState}"/>.
    /// </summary>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class OperationUpdateContext<TOperationState>
    {
        /// <summary>
        /// Gets current state of operation.
        /// </summary>
        public IOperation<TOperationState> Operation { get; }

        /// <summary>
        /// Gets or sets new state for operation.
        /// </summary>
        [MaybeNull]
        public TOperationState NewState { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationUpdateContext{TOperationState}"/> class.
        /// </summary>
        /// <param name="operation">Current operation state.</param>
        public OperationUpdateContext(IOperation<TOperationState> operation)
        {
            Operation = operation;
        }
    }
}
