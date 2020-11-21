// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks.Dataflow;

namespace MicroElements.Processing.Pipelines
{
    /// <summary>
    /// Represents pipeline step configuration.
    /// </summary>
    public class StepSettings
    {
        /// <summary>
        /// Gets <see cref="ExecutionDataflowBlockOptions"/> for step.
        /// By default MaxDegreeOfParallelism == 1.
        /// </summary>
        public ExecutionDataflowBlockOptions ExecutionOptions { get; } = new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = 1,
        };

        /// <summary>
        /// Gets <see cref="DataflowLinkOptions"/> for step linking.
        /// </summary>
        public DataflowLinkOptions LinkOptions { get; } = new DataflowLinkOptions()
        {
            PropagateCompletion = true,
        };

        /// <summary>
        /// Gets or sets the maximum number of messages that may be processed by the block concurrently.
        /// </summary>
        public int MaxDegreeOfParallelism
        {
            get { return ExecutionOptions.MaxDegreeOfParallelism; }
            set { ExecutionOptions.MaxDegreeOfParallelism = value; }
        }
    }
}
