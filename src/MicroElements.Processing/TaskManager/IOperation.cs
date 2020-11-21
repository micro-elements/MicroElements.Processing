// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Metadata;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents single operation (task, unit of work).
    /// </summary>
    public interface IOperation : IMetadataProvider
    {
        /// <summary>
        /// Gets operation id.
        /// </summary>
        OperationId Id { get; }

        /// <summary>
        /// Gets operation status.
        /// </summary>
        OperationStatus Status { get; }

        /// <summary>
        /// Gets date and time of start.
        /// </summary>
        LocalDateTime? StartedAt { get; }

        /// <summary>
        /// Gets date and time of finish.
        /// </summary>
        LocalDateTime? FinishedAt { get; }

        /// <summary>
        /// Gets operation state type.
        /// </summary>
        Type StateType { get; }

        /// <summary>
        /// Gets operation state as object.
        /// </summary>
        object StateUntyped { get; }

        /// <summary>
        /// Gets exception occurred on task execution.
        /// </summary>
        Exception? Exception { get; }
    }

    /// <summary>
    /// Represents single operation with strong typed state.
    /// </summary>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface IOperation<out TOperationState> : IOperation
    {
        /// <summary>
        /// Gets operation state.
        /// </summary>
        [NotNull]
        TOperationState State { get; }
    }
}
