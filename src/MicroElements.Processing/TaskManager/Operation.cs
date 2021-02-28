// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;
using MicroElements.Metadata;
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
        public IPropertyContainer Metadata { get; }

        /// <inheritdoc />
        public OperationId Id { get; }

        /// <inheritdoc />
        [NotNull]
        public TOperationState State { get; }

        /// <inheritdoc />
        public Type StateType => typeof(TOperationState);

        /// <inheritdoc />
        public object StateUntyped => State;

        /// <inheritdoc />
        public OperationStatus Status { get; }

        /// <inheritdoc />
        public LocalDateTime? StartedAt { get; }

        /// <inheritdoc />
        public LocalDateTime? FinishedAt { get; }

        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Operation{TOperationState}"/> class.
        /// </summary>
        /// <param name="id">Operation id.</param>
        /// <param name="state">Operation state.</param>
        /// <param name="status">Operation status.</param>
        /// <param name="startedAt">Date and time of start.</param>
        /// <param name="finishedAt">Date and time of finish.</param>
        /// <param name="exception">Exception occured on task execution.</param>
        /// <param name="metadata">Optional metadata.</param>
        public Operation(
            OperationId id,
            [DisallowNull] TOperationState state,
            OperationStatus status,
            LocalDateTime? startedAt,
            LocalDateTime? finishedAt,
            Exception? exception,
            IPropertyContainer? metadata)
        {
            state.AssertArgumentNotNull(nameof(state));

            Id = id;
            State = state;
            Status = status;

            StartedAt = startedAt;
            FinishedAt = finishedAt;
            Exception = exception;

            if (metadata == null)
            {
                // If state is metadata provider itself then use it as operation metadata.
                if (state is IMetadataProvider stateMetadataProvider &&
                    stateMetadataProvider.GetInstanceMetadata(autoCreate: false) is { } stateMetadata)
                    Metadata = stateMetadata.ToReadOnly();
                else
                    Metadata = PropertyContainer.Empty;
            }
            else
            {
                // ReadOnly metadata
                Metadata = new PropertyContainer(metadata);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string optionalException = Exception != null ? $", {nameof(Exception)}: {Exception?.Message}" : string.Empty;
            return $"{nameof(Id)}: {Id}, {nameof(Status)}: {Status}{optionalException}";
        }
    }

    /// <summary>
    /// Static helper methods.
    /// </summary>
    public static class Operation
    {
        /// <summary>
        /// Creates new instance of <see cref="IOperation{TOperationState}"/> in <see cref="OperationStatus.NotStarted"/>.
        /// </summary>
        /// <typeparam name="TOperationState">Operation state.</typeparam>
        /// <param name="id">Operation id.</param>
        /// <param name="state">Initial operation state.</param>
        /// <param name="metadata">Optional metadata.</param>
        /// <returns>Created operation.</returns>
        public static IOperation<TOperationState> CreateNotStarted<TOperationState>(
            OperationId id,
            [DisallowNull] TOperationState state,
            IPropertyContainer? metadata = null)
        {
            return new Operation<TOperationState>(
                id: id,
                state: state,
                status: OperationStatus.NotStarted,
                startedAt: default,
                finishedAt: default,
                exception: null,
                metadata: metadata);
        }
    }
}
