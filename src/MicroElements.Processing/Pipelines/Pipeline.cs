// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MicroElements.Processing.Common;

namespace MicroElements.Processing.Pipelines
{
    /// <summary>
    /// Pipeline for type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Input pipeline type.</typeparam>
    public class Pipeline<T> : IPipeline<T>
    {
        private readonly List<IDataflowBlock> _blocks = new List<IDataflowBlock>();

        /// <inheritdoc/>
        public ITargetBlock<T> Input { get; }

        internal ISourceBlock<T> Last => (ISourceBlock<T>)_blocks.Last();

        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline{T}"/> class.
        /// </summary>
        public Pipeline()
        {
            Input = new BufferBlock<T>();
            _blocks.Add(Input);
        }

        /// <inheritdoc />
        public Task CompleteAndWait()
        {
            Input.Complete();
            return GetPipelineCompletion();
        }

        /// <summary>
        /// Gets pipeline completion task.
        /// </summary>
        /// <returns>Pipeline completion task.</returns>
        public Task GetPipelineCompletion()
        {
            Task[] completionTasks = _blocks.Select(block => block.Completion).ToArray();
            Task pipelineCompletionTask = completionTasks.AwaitableWhenAll();
            return pipelineCompletionTask;
        }

        /// <summary>
        /// Adds new step to pipeline.
        /// </summary>
        /// <param name="step">Async step function.</param>
        /// <param name="configure">Optional step configure action.</param>
        /// <returns>The same pipeline for chaining.</returns>
        public Pipeline<T> AddStep(Func<T, Task<T>> step, Action<StepSettings>? configure = null)
        {
            var settings = CreateAndConfigureStepSettings(configure);

            var transformBlock = new TransformBlock<T, T>(step, settings.ExecutionOptions);
            Last.LinkTo(transformBlock, settings.LinkOptions);
            _blocks.Add(transformBlock);
            return this;
        }

        /// <summary>
        /// Adds new step to pipeline.
        /// </summary>
        /// <param name="step">Step action.</param>
        /// <param name="configure">Optional step configure action.</param>
        /// <returns>The same pipeline for chaining.</returns>
        public Pipeline<T> AddStep(Action<T> step, Action<StepSettings>? configure = null)
        {
            var settings = CreateAndConfigureStepSettings(configure);

            var actionBlock = new ActionBlock<T>(step, settings.ExecutionOptions);
            Last.LinkTo(actionBlock, settings.LinkOptions);
            _blocks.Add(actionBlock);
            return this;
        }

        /// <summary>
        /// Adds new step to pipeline.
        /// </summary>
        /// <param name="step">Async step action.</param>
        /// <param name="configure">Optional step configure action.</param>
        /// <returns>The same pipeline for chaining.</returns>
        public Pipeline<T> AddStep(Func<T, Task> step, Action<StepSettings>? configure = null)
        {
            var settings = CreateAndConfigureStepSettings(configure);

            var actionBlock = new ActionBlock<T>(step, settings.ExecutionOptions);
            Last.LinkTo(actionBlock, settings.LinkOptions);
            _blocks.Add(actionBlock);
            return this;
        }

        private static StepSettings CreateAndConfigureStepSettings(Action<StepSettings>? configure)
        {
            var settings = new StepSettings();
            configure?.Invoke(settings);
            return settings;
        }
    }
}
