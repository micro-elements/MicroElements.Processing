// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MicroElements.Processing.TaskManager.Metrics
{
    /// <summary>
    /// Session telemetry.
    /// </summary>
    public class SessionTracer : ITracer, IDisposable
    {
        /// <summary>
        /// Gets global static <see cref="ActivitySource"/>.
        /// </summary>
        public static ActivitySource Processing { get; } = new ActivitySource(name: "MicroElements.Processing");

        private readonly ActivityListener _activityListener;

        /// <inheritdoc />
        public ActivitySource ActivitySource { get; }

        /// <inheritdoc />
        public Activity? MainActivity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTracer"/> class.
        /// </summary>
        /// <param name="logger">Logger to log activities.</param>
        /// <param name="tags">Session tags.</param>
        public SessionTracer(
            ILogger logger,
            IReadOnlyCollection<KeyValuePair<string, object?>>? tags = null)
        {
            ActivitySource = Processing;

            // If no listener added - no activity will be created. Add listeners in host.
            _activityListener = new LoggingActivityListener(logger, ActivitySource).Listener;

            // Main activity.
            MainActivity = ActivitySource.StartActivity(
                name: "Session",
                kind: ActivityKind.Internal,
                parentId: null!,
                tags: tags);
        }

        /// <inheritdoc />
        public Activity? StartActivity(string activityName, IEnumerable<KeyValuePair<string, object?>>? tags = null)
        {
            return ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: this.GetMainActivityId() !, tags: tags);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            MainActivity?.Dispose();
            _activityListener.Dispose();
        }
    }
}
