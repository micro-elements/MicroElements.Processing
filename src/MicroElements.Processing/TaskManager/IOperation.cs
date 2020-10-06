// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using MicroElements.Metadata;
using NodaTime;
using NodaTime.Extensions;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents single untyped operation (task).
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
        /// Gets exception occured on task execution.
        /// </summary>
        Exception? Exception { get; }
    }

    /// <summary>
    /// Represents single operation with state.
    /// </summary>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface IOperation<TOperationState> : IOperation
    {
        /// <summary>
        /// Gets operation state.
        /// </summary>
        TOperationState State { get; }
    }

    public static class Operation
    {
        public static Operation<State> CreateNotStarted<State>(
            OperationId id,
            State state)
        {
            return new Operation<State>(
                id: id,
                state: state,
                status: OperationStatus.NotStarted,
                startedAt: default,
                finishedAt: default,
                exception: null);
        }

        public static Operation<State> CreateInProgress<State>(
            OperationId id,
            State state)
        {
            return new Operation<State>(
                id: id,
                state: state,
                status: OperationStatus.InProgress,
                startedAt: DateTime.Now.ToLocalDateTime(),
                finishedAt: default,
                exception: null);
        }

        public static Operation<State> Create<State>(
            IOperation<State> value,
            OperationStatus? status = default,
            State state = default,
            LocalDateTime? startedAt = default,
            LocalDateTime? finishedAt = default,
            Exception? exception = null)
        {
            return new Operation<State>
            (
                id: value.Id,
                state: !state.IsDefault() ? state : value.State,
                status: status ?? value.Status,
                startedAt: startedAt ?? value.StartedAt,
                finishedAt: finishedAt ?? value.FinishedAt,
                exception: exception ?? value.Exception
            );
        }

        /// <summary>
        /// Returns the new <see cref="Operation{State}"/> with updated <paramref name="status"/>.
        /// </summary>
        public static IOperation<State> WithStatus<State>(this IOperation<State> value, OperationStatus status)
            => Operation.Create(value, status: status);

        /// <summary>
        /// Returns the new <see cref="Operation{State}"/> with updated <paramref name="context"/>.
        /// </summary>
        public static IOperation<State> WithState<State>(this IOperation<State> value, State context)
            => Operation.Create(value, state: context);

        /// <summary>
        /// Returns the new <see cref="Operation{State}"/> with updated <paramref name="startedAt"/>.
        /// </summary>
        public static IOperation<State> WithStartedAt<State>(this IOperation<State> value, LocalDateTime startedAt)
            => Operation.Create(value, startedAt: startedAt);

        /// <summary>
        /// Returns the new <see cref="Operation{State}"/> with updated <paramref name="finishedAt"/>.
        /// </summary>
        public static IOperation<State> WithFinishedAt<State>(this IOperation<State> value, LocalDateTime finishedAt)
            => Operation.Create(value, finishedAt: finishedAt);

        /// <summary>
        /// Returns the new <see cref="Operation{State}"/> with updated <paramref name="finishedAt"/>.
        /// </summary>
        public static IOperation<State> WithException<State>(this IOperation<State> value, Exception exception)
            => Operation.Create(value, exception: exception);

        /// <summary>
        /// Длительность операции.
        /// </summary>
        public static Duration? GetElapsed<State>(this IOperation<State> value)
        {
            if (value.StartedAt != null)
                return (value.FinishedAt.GetValueOrDefault(DateTime.Now.ToLocalDateTime()) - value.StartedAt.Value).ToDuration();
            return default;
        }
    }
}
