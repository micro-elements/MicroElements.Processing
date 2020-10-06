// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Execution options for executing tasks in <see cref="IOperationManager{TSessionState,TOperationState}"/>.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    /// <typeparam name="TOperationState">Operation state.</typeparam>
    public class ExecutionOptions<TSessionState, TOperationState>
    {
        /// <summary>
        /// Gets executor for operations.
        /// </summary>
        public ITaskExecutor<TSessionState, TOperationState> Executor { get; }

        /// <summary>
        /// Gets max level of concurrency.
        /// </summary>
        public int MaxConcurrencyLevel { get; }

        /// <summary>
        /// Gets timeout for entire session.
        /// </summary>
        public TimeSpan SessionTimeout { get; }

        /// <summary>
        /// Gets action fired on session finished.
        /// </summary>
        public Action<ISession<TSessionState, TOperationState>>? OnFinished { get; }

        /// <summary>
        /// Optional CancellationToken.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionOptions{TSessionState, TOperationState}"/> class.
        /// </summary>
        /// <param name="executor">Executor for operations.</param>
        /// <param name="onFinished">Max level of concurrency.</param>
        /// <param name="maxDegreeOfParallelism">Timeout for entire session.</param>
        /// <param name="totalPipelineTimeout">Action fired on session finished.</param>
        /// <param name="cancellationToken">Optional CancellationToken for execution session.</param>
        public ExecutionOptions(
            ITaskExecutor<TSessionState, TOperationState> executor,
            Action<ISession<TSessionState, TOperationState>>? onFinished = null,
            int? maxDegreeOfParallelism = null,
            TimeSpan? totalPipelineTimeout = null,
            CancellationToken cancellationToken = default)
        {
            Executor = executor;
            OnFinished = onFinished;
            MaxConcurrencyLevel = maxDegreeOfParallelism ?? Environment.ProcessorCount;
            SessionTimeout = totalPipelineTimeout ?? TimeSpan.FromHours(24);
            CancellationToken = cancellationToken;
        }
    }
}
