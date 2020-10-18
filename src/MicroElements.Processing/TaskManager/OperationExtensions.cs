// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;
using MicroElements.Metadata;
using NodaTime;
using NodaTime.Extensions;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Extension methods for <see cref="IOperation"/>.
    /// </summary>
    public static class OperationExtensions
    {
        /// <summary>
        /// Creates new instance of operation with changed properties.
        /// </summary>
        /// <typeparam name="TOperationState">Operation state.</typeparam>
        /// <param name="operation">Source operation.</param>
        /// <param name="id">New id.</param>
        /// <param name="status">New status.</param>
        /// <param name="state">New state.</param>
        /// <param name="startedAt">New startedAt.</param>
        /// <param name="finishedAt">New finishedAt.</param>
        /// <param name="exception">New exception.</param>
        /// <param name="metadata">New metadata.</param>
        /// <returns>New instance of operation with changed properties.</returns>
        public static Operation<TOperationState> With<TOperationState>(
            this IOperation<TOperationState> operation,
            OperationId? id = default,
            OperationStatus? status = default,
            [AllowNull] TOperationState state = default,
            LocalDateTime? startedAt = default,
            LocalDateTime? finishedAt = default,
            Exception? exception = null,
            IPropertyContainer? metadata = null)
        {
            return new Operation<TOperationState>
            (
                id: id ?? operation.Id,
                state: state.IsNotNull() ? state : operation.State,
                status: status ?? operation.Status,
                startedAt: startedAt ?? operation.StartedAt,
                finishedAt: finishedAt ?? operation.FinishedAt,
                exception: exception ?? operation.Exception,
                metadata: metadata ?? operation.Metadata
            );
        }

        /// <summary>
        /// Gets operation duration. Works for operations in any status.
        /// </summary>
        /// <param name="operation">Source operation.</param>
        /// <returns>Duration for operation.</returns>
        public static Duration GetDuration(this IOperation operation)
        {
            var finishedAt = operation.FinishedAt.GetValueOrDefault(DateTime.Now.ToLocalDateTime());
            var startedAt = operation.StartedAt.GetValueOrDefault(finishedAt);
            return (finishedAt - startedAt).ToDuration();
        }
    }
}
