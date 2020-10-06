// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
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
    public class Session<TSessionState, TOperationState> : ISession<TSessionState, TOperationState>
    {
        /// <inheritdoc />
        public OperationId Id { get; }

        /// <inheritdoc />
        public OperationStatus Status { get; }

        /// <inheritdoc />
        public LocalDateTime? StartedAt { get; }

        /// <inheritdoc />
        public LocalDateTime? FinishedAt { get; }

        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public TSessionState State { get; }

        /// <inheritdoc />
        public IMutableMessageList<Message> Messages { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IOperation<TOperationState>> Operations { get; }

        /// <inheritdoc />
        public IEnumerator<IOperation> GetEnumerator()
        {
            return Operations.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Session(
            OperationId id,
            OperationStatus status,
            TSessionState state,
            LocalDateTime? startedAt,
            LocalDateTime? finishedAt,
            Exception? exception,
            IMutableMessageList<Message>? messages,
            IReadOnlyCollection<IOperation<TOperationState>>? operations)
        {
            Id = id;
            Status = status;
            State = state;

            StartedAt = startedAt;
            FinishedAt = finishedAt;
            Exception = exception;
            Messages = messages ?? new ConcurrentMessageList<Message>();
            Operations = operations ?? Array.Empty<IOperation<TOperationState>>();
        }
    }

    public static class Session
    {
        public static ISession<TSessionState> Create<TSessionState>(
            OperationId id,
            OperationStatus status,
            TSessionState state = default,
            LocalDateTime? startedAt = null,
            LocalDateTime? finishedAt = null,
            Exception? exception = null,
            IMutableMessageList<Message> messages = null)
        {
            return new Session<TSessionState, Unit>(
                id: id,
                state: state,
                status: status,
                startedAt: startedAt,
                finishedAt: finishedAt,
                exception: exception,
                messages: messages,
                operations: null);
        }

        public static ISession<TSessionState, TOperationState> Create<TSessionState, TOperationState>(
            OperationId id,
            OperationStatus status,
            TSessionState state = default,
            LocalDateTime? startedAt = null,
            LocalDateTime? finishedAt = null,
            Exception? exception = null,
            IMutableMessageList<Message>? messages = null,
            IReadOnlyCollection<IOperation<TOperationState>>? operations = null)
        {
            return new Session<TSessionState, TOperationState>(
                id: id,
                state: state,
                status: status,
                startedAt: startedAt,
                finishedAt: finishedAt,
                exception: exception,
                messages: messages,
                operations: operations);
        }

        public static ISession<TSessionState> With<TSessionState>(
            this ISession<TSessionState> session,
            OperationId? id = null,
            TSessionState state = default,
            OperationStatus? status = null,
            LocalDateTime? startedAt = null,
            LocalDateTime? finishedAt = null,
            Exception? exception = null,
            IMutableMessageList<Message>? messages = null)
        {
            return Create(
                id: id ?? session.Id,
                state: state ?? session.State,
                status: status ?? session.Status,
                startedAt: startedAt ?? session.StartedAt,
                finishedAt: finishedAt ?? session.FinishedAt,
                exception: exception ?? session.Exception,
                messages: messages ?? session.Messages);
        }

        public static ISession<TSessionState, TOperationState> WithOperations<TSessionState, TOperationState>(
            this ISession<TSessionState> session,
            IReadOnlyCollection<IOperation<TOperationState>> operations)
        {
            return new Session<TSessionState, TOperationState>(
                id: session.Id,
                state: session.State,
                status: session.Status,
                startedAt: session.StartedAt,
                finishedAt: session.FinishedAt,
                exception: session.Exception,
                messages: session.Messages,
                operations: operations);
        }
    }
}
