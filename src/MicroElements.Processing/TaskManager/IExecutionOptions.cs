﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Execution options for <see cref="IOperationManager"/>.
    /// </summary>
    public interface IExecutionOptions
    {
        /// <summary>
        /// Gets max level of concurrency.
        /// </summary>
        int MaxConcurrencyLevel { get; }

        /// <summary>
        /// Gets timeout for entire session.
        /// </summary>
        TimeSpan SessionTimeout { get; }
    }

    /// <summary>
    /// Execution options for <see cref="IOperationManager"/>.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public interface IExecutionOptions<TSessionState, TOperationState> : IExecutionOptions
    {
        /// <summary>
        /// Gets optional CancellationToken.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets executor for operations.
        /// </summary>
        IOperationExecutor<TSessionState, TOperationState> Executor { get; }

        /// <summary>
        /// Gets action fired on session finished.
        /// </summary>
        Action<ISession<TSessionState, TOperationState>>? OnSessionFinished { get; }

        /// <summary>
        /// Gets action fired on operation finished.
        /// </summary>
        Action<ISession<TSessionState>, IOperation<TOperationState>>? OnOperationFinished { get; }
    }

    /// <summary>
    /// Execution options for executing tasks in <see cref="IOperationManager{TSessionState,TOperationState}"/>.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class ExecutionOptions<TSessionState, TOperationState> : IExecutionOptions<TSessionState, TOperationState>
    {
        /// <inheritdoc />
        public int MaxConcurrencyLevel { get; set; } = Environment.ProcessorCount;

        /// <inheritdoc />
        public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(24);

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <inheritdoc />
        public IOperationExecutor<TSessionState, TOperationState> Executor { get; set; }

        /// <inheritdoc />
        public Action<ISession<TSessionState, TOperationState>>? OnSessionFinished { get; set; }

        /// <inheritdoc />
        public Action<ISession<TSessionState>, IOperation<TOperationState>>? OnOperationFinished { get; set; }
    }
}
