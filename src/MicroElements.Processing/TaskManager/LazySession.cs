// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;
using MicroElements.Metadata;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Session contains session state and status.
    /// Lazy session does not get operations until you need.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    public class LazySession<TSessionState> : ISession<TSessionState>
    {
        /// <inheritdoc />
        public IPropertyContainer Metadata => Operation.Metadata;

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
        public object StateUntyped => Operation.StateUntyped;

        /// <inheritdoc />
        [NotNull]
        public TSessionState State => Operation.State;

        /// <inheritdoc />
        public IOperation<TSessionState> Operation { get; }

        /// <inheritdoc />
        public IMutableMessageList<Message> Messages { get; }

        /// <inheritdoc />
        public Func<IReadOnlyCollection<IOperation>> GetOperations { get; }

        /// <inheritdoc />
        public IExecutionOptions? ExecutionOptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazySession{TSessionState}"/> class.
        /// </summary>
        /// <param name="sessionOperation">Operation that holds operation part of session.</param>
        /// <param name="messages">Optional messages.</param>
        /// <param name="getOperations">Optional lazy operations getter.</param>
        /// <param name="executionOptions">Session execution options.</param>
        public LazySession(
            IOperation<TSessionState> sessionOperation,
            IMutableMessageList<Message> messages,
            Func<IReadOnlyCollection<IOperation>> getOperations,
            IExecutionOptions? executionOptions)
        {
            Operation = sessionOperation.AssertArgumentNotNull(nameof(sessionOperation));
            Messages = messages.AssertArgumentNotNull(nameof(messages));
            GetOperations = getOperations.AssertArgumentNotNull(nameof(getOperations));
            ExecutionOptions = executionOptions;
        }
    }
}
