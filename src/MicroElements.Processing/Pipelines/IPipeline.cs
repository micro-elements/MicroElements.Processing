// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        /// Gets pipeline input block.
        /// </summary>
        ITargetBlock<T> Input { get; }

        /// <summary>
        /// Complete input and wait all blocks completion.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CompleteAndWait();
    }
}
