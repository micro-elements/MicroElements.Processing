// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MicroElements.Processing.TaskManager.Metrics
{
    /// <summary>
    /// Creates and starts child activity.
    /// </summary>
    public class ChildTracer : ITracer, IDisposable
    {
        /// <inheritdoc />
        public ActivitySource ActivitySource { get; }

        /// <inheritdoc />
        public Activity? MainActivity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildTracer"/> class.
        /// Creates and starts child activity.
        /// </summary>
        /// <param name="parentTracer">Parent tracer.</param>
        /// <param name="activityName">Activity name.</param>
        /// <param name="tags">Optional activity tags.</param>
        public ChildTracer(ITracer parentTracer, string activityName, IReadOnlyCollection<KeyValuePair<string, object?>>? tags = null)
        {
            ActivitySource = parentTracer.ActivitySource;
            MainActivity = parentTracer.StartActivity(activityName, tags);
        }

        /// <inheritdoc />
        public Activity? StartActivity(string activityName, IEnumerable<KeyValuePair<string, object?>>? tags = null)
        {
            return ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: this.GetMainActivityId(), tags: tags);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            MainActivity?.Dispose();
        }
    }
}
