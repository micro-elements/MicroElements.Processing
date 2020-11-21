// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MicroElements.Processing.Pipelines
{
    /// <summary>
    /// Useful pipeline extensions.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Posts all items to <paramref name="inputBlock"/>.
        /// </summary>
        /// <typeparam name="TInput">Specifies the type of data accepted by the <see cref="ITargetBlock{TInput}"/>.</typeparam>
        /// <param name="inputBlock">Input block.</param>
        /// <param name="items">Items to post.</param>
        public static void PostMany<TInput>(this ITargetBlock<TInput> inputBlock, IEnumerable<TInput> items)
        {
            foreach (TInput item in items)
            {
                inputBlock.Post(item);
            }
        }

        /// <summary>
        /// Posts all items to <paramref name="pipeline"/>.
        /// </summary>
        /// <typeparam name="TInput">Specifies the type of data accepted by <paramref name="pipeline"/>.</typeparam>
        /// <param name="pipeline">Pipeline.</param>
        /// <param name="items">Items to post.</param>
        public static void PostMany<TInput>(this IPipeline<TInput> pipeline, IEnumerable<TInput> items)
        {
            pipeline.Input.PostMany(items);
        }

        /// <summary>
        /// Sends all items to <paramref name="inputBlock"/>.
        /// </summary>
        /// <typeparam name="TInput">Specifies the type of data accepted by the <see cref="ITargetBlock{TInput}"/>.</typeparam>
        /// <param name="inputBlock">Input block.</param>
        /// <param name="items">Items to post.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SendManyAsync<TInput>(this ITargetBlock<TInput> inputBlock, IEnumerable<TInput> items)
        {
            foreach (TInput item in items)
            {
                await inputBlock.SendAsync(item);
            }
        }

        /// <summary>
        /// Sends all items to <paramref name="pipeline"/>.
        /// </summary>
        /// <typeparam name="TInput">Specifies the type of data accepted by <paramref name="pipeline"/>.</typeparam>
        /// <param name="pipeline">Pipeline.</param>
        /// <param name="items">Items to post.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task SendManyAsync<TInput>(this IPipeline<TInput> pipeline, IEnumerable<TInput> items)
        {
            return pipeline.Input.SendManyAsync(items);
        }
    }
}
