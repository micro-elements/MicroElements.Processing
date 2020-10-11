// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Session that contains multiple tasks to process.
    /// </summary>
    /// <typeparam name="TSessionState">Session state type.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class MaterializedSession<TSessionState, TOperationState> : ISession<TSessionState, TOperationState>
    {
        /// <inheritdoc />
        public OperationId Id => Operation.Id;

        /// <inheritdoc />
        public OperationStatus Status => Operation.Status;

        /// <inheritdoc />
        public LocalDateTime? StartedAt => Operation.StartedAt;

        /// <inheritdoc />
        public LocalDateTime? FinishedAt => Operation.FinishedAt;

        /// <inheritdoc />
        public Exception? Exception => Operation.Exception;

        /// <inheritdoc />
        public Type StateType => Operation.StateType;

        /// <inheritdoc />
        public object? StateUntyped => Operation.StateUntyped;

        /// <inheritdoc />
        public TSessionState State => Operation.State;

        /// <inheritdoc />
        public IOperation<TSessionState> Operation { get; }

        /// <inheritdoc />
        public IMutableMessageList<Message> Messages { get; }

        /// <inheritdoc />
        public Func<IReadOnlyCollection<IOperation>> GetOperations { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IOperation<TOperationState>> Operations { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterializedSession{TSessionState,TOperationState}"/> class.
        /// </summary>
        /// <param name="sessionOperation">Session state.</param>
        /// <param name="messages">Messages.</param>
        /// <param name="operations">Operations.</param>
        public MaterializedSession(
            IOperation<TSessionState> sessionOperation,
            IMutableMessageList<Message> messages,
            IReadOnlyCollection<IOperation<TOperationState>> operations)
        {
            Operation = sessionOperation.AssertArgumentNotNull(nameof(sessionOperation));
            Messages = messages.AssertArgumentNotNull(nameof(messages));
            Operations = operations.AssertArgumentNotNull(nameof(operations));
            GetOperations = () => Operations;
        }
    }
}
