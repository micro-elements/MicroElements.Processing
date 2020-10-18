// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Metadata;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Creates new session from operation.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <param name="operation">Session state operation.</param>
        /// <param name="getOperations">Get operations function.</param>
        /// <param name="messages">Optional messages list.</param>
        /// <returns>New instance of session.</returns>
        public static ISession<TSessionState> ToSession<TSessionState>(
            this IOperation<TSessionState> operation,
            Func<IReadOnlyCollection<IOperation>> getOperations,
            IMutableMessageList<Message>? messages = null)
        {
            return new LazySession<TSessionState>(
                sessionOperation: operation,
                getOperations: getOperations,
                messages: messages ?? new ConcurrentMessageList<Message>(),
                executionOptions: null);
        }

        /// <summary>
        /// Creates new instance of <see cref="ISession{TSessionState}"/> with changes.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <param name="source">Source session.</param>
        /// <param name="id">New session id.</param>
        /// <param name="state">New session state.</param>
        /// <param name="status">New session status.</param>
        /// <param name="startedAt">New startedAt.</param>
        /// <param name="finishedAt">New finishedAt.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="metadata">Metadata for session.</param>
        /// <param name="executionOptions">Session execution options.</param>
        /// <returns>New instance of session.</returns>
        public static ISession<TSessionState> With<TSessionState>(
            this ISession<TSessionState> source,
            OperationId? id = null,
            TSessionState state = default,
            OperationStatus? status = null,
            LocalDateTime? startedAt = null,
            LocalDateTime? finishedAt = null,
            Exception? exception = null,
            IPropertyContainer? metadata = null,
            IExecutionOptions? executionOptions = null)
        {
            var sessionOperation = source.Operation.With(
                    id: id,
                    state: state,
                    status: status,
                    startedAt: startedAt,
                    finishedAt: finishedAt,
                    exception: exception,
                    metadata: metadata);

            return new LazySession<TSessionState>(
                sessionOperation: sessionOperation,
                messages: source.Messages,
                getOperations: source.GetOperations,
                executionOptions: executionOptions ?? source.ExecutionOptions);
        }

        /// <summary>
        /// Creates new materialized session with operations.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <typeparam name="TOperationState">Operation state.</typeparam>
        /// <param name="source">Source session.</param>
        /// <param name="operations">Operation list.</param>
        /// <returns>New session with operations.</returns>
        public static ISession<TSessionState, TOperationState> WithOperations<TSessionState, TOperationState>(
            this ISession<TSessionState> source,
            IReadOnlyCollection<IOperation<TOperationState>>? operations = null)
        {
            operations ??= source.GetOperations().Cast<IOperation<TOperationState>>().ToArray();

            return new MaterializedSession<TSessionState, TOperationState>(
                sessionOperation: source.Operation,
                messages: source.Messages,
                operations: operations,
                executionOptions: source.ExecutionOptions);
        }
    }
}
