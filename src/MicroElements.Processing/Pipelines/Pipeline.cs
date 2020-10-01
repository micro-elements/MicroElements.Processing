// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MicroElements.Processing.Pipelines
{
    /// <summary>
    /// Represents typed dataflow pipeline.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    public interface IPipeline<T>
    {
        /// <summary>
        /// Pipeline input.
        /// </summary>
        ITargetBlock<T> Input { get; }

        /// <summary>
        /// Complete input and wait all blocks completion.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CompleteAndWait();
    }

    public class Pipeline<T> : IPipeline<T>
    {
        private readonly List<IDataflowBlock> _blocks = new List<IDataflowBlock>();

        /// <inheritdoc/>
        public ITargetBlock<T> Input { get; }

        internal ISourceBlock<T> Last => (ISourceBlock<T>) _blocks.Last();

        public Pipeline()
        {
            Input = new BufferBlock<T>();
            _blocks.Add(Input);
        }

        public Task CompleteAndWait()
        {
            Input.Complete();
            return GetPipelineCompletion();
        }

        public Task GetPipelineCompletion()
        {
            Task[] completionTasks = _blocks.Select(block => block.Completion).ToArray();
            Task pipelineCompletionTask = completionTasks.AwaitableWhenAll();
            return pipelineCompletionTask;
        }

        private static StepSettings StepSettings(Action<StepSettings>? configure)
        {
            var settings = new StepSettings();
            configure?.Invoke(settings);
            return settings;
        }

        public Pipeline<T> AddStep(Func<T, Task<T>> step, Action<StepSettings>? configure = null)
        {
            var settings = StepSettings(configure);

            var transformBlock = new TransformBlock<T, T>(step, settings.ExecutionOptions);
            Last.LinkTo(transformBlock, settings.LinkOptions);
            _blocks.Add(transformBlock);
            return this;
        }

        public Pipeline<T> AddStep(Action<T> step, Action<StepSettings>? configure = null)
        {
            var settings = StepSettings(configure);

            var actionBlock = new ActionBlock<T>(step, settings.ExecutionOptions);
            Last.LinkTo(actionBlock, settings.LinkOptions);
            _blocks.Add(actionBlock);
            return this;
        }

        public Pipeline<T> AddStep(Func<T, Task> step, Action<StepSettings>? configure = null)
        {
            var settings = StepSettings(configure);

            var actionBlock = new ActionBlock<T>(step, settings.ExecutionOptions);
            Last.LinkTo(actionBlock, settings.LinkOptions);
            _blocks.Add(actionBlock);
            return this;
        }
    }

    public class StepSettings
    {
        public ExecutionDataflowBlockOptions ExecutionOptions { get; } = new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = 1,
        };

        public DataflowLinkOptions LinkOptions { get; } = new DataflowLinkOptions()
        {
            PropagateCompletion = true,
        };

        /// <summary>Gets the maximum number of messages that may be processed by the block concurrently.</summary>
        public int MaxDegreeOfParallelism
        {
            get { return ExecutionOptions.MaxDegreeOfParallelism; }
            set { ExecutionOptions.MaxDegreeOfParallelism = value; }
        }

    }
}
