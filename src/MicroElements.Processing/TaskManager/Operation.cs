// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Operation with state.
    /// </summary>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class Operation<TOperationState> : IOperation<TOperationState>
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
        public TOperationState State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Operation{TOperationState}"/> class.
        /// </summary>
        /// <param name="id">Operation id.</param>
        /// <param name="state">Operation state.</param>
        /// <param name="status">Operation status.</param>
        /// <param name="startedAt">Date and time of start.</param>
        /// <param name="finishedAt">Date and time of finish.</param>
        /// <param name="exception">Exception occured on task execution.</param>
        public Operation(
            OperationId id,
            TOperationState state,
            OperationStatus status,
            LocalDateTime? startedAt,
            LocalDateTime? finishedAt,
            Exception? exception)
        {
            Id = id;
            State = state;

            Status = status;
            StartedAt = startedAt;
            FinishedAt = finishedAt;
            Exception = exception;
        }
    }
}
