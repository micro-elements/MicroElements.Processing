// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace MicroElements.Processing.TaskManager.Metrics
{
    /// <summary>
    /// Represents tracing recorder.
    /// See: OpenTelemetry.
    /// </summary>
    public interface ITracer
    {
        /// <summary>
        /// Gets <see cref="ActivitySource"/> for this tracer.
        /// </summary>
        ActivitySource ActivitySource { get; }

        /// <summary>
        /// Gets main tracer activity.
        /// </summary>
        Activity? MainActivity { get; }

        /// <summary>
        /// Starts activity.
        /// Returns not null if <see cref="ActivitySource"/> has listeners.
        /// </summary>
        /// <param name="activityName">Activity name.</param>
        /// <param name="tags">Tags to attach to activity.</param>
        /// <returns>Optional activity.</returns>
        Activity? StartActivity(string activityName, IEnumerable<KeyValuePair<string, object?>>? tags = null);
    }

    /// <summary>
    /// Tracer extensions.
    /// </summary>
    public static class TracerExtensions
    {
        /// <summary>
        /// Gets optional MainActivity Id for tracer.
        /// </summary>
        /// <param name="tracer">Source tracer.</param>
        /// <returns>Optional ActivityId.</returns>
        public static string? GetMainActivityId(this ITracer tracer)
        {
            return tracer.MainActivity?.Id;
        }
    }
}
