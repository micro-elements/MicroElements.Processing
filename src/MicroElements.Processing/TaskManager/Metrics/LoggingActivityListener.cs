// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using MicroElements.Functional;
using Microsoft.Extensions.Logging;

namespace MicroElements.Processing.TaskManager.Metrics
{
    /// <summary>
    /// Creates simple activity listener that logs activities.
    /// </summary>
    public class LoggingActivityListener
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Gets created listener.
        /// </summary>
        public ActivityListener Listener { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingActivityListener"/> class.
        /// </summary>
        /// <param name="logger">Logger to log activities.</param>
        /// <param name="activitySource"><see cref="ActivitySource"/> that will work with listener.</param>
        public LoggingActivityListener(ILogger logger, ActivitySource activitySource)
        {
            _logger = logger;

            Listener = new ActivityListener
            {
                ActivityStarted = ActivityStarted,
                ActivityStopped = ActivityStopped,
                ShouldListenTo = source => source == activitySource,
                Sample = Sample,
                SampleUsingParentId = SampleUsingParentId,
            };

            ActivitySource.AddActivityListener(Listener);
        }

        private void ActivityStarted(Activity activity)
        {
            _logger.LogInformation($"Activity started: Name: {activity.OperationName}, Id: {activity.Id}, Tags: {FormatTags(activity)}");
        }

        private void ActivityStopped(Activity activity)
        {
            _logger.LogInformation($"Activity finished: Name: {activity.OperationName}, Id: {activity.Id}, Tags: {FormatTags(activity)}, Duration: {activity.Duration}");
        }

        private string FormatTags(Activity activity) => activity.Tags != null ? activity.Tags.FormatAsTuple() : "null";

        private ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> options)
        {
            return ActivitySamplingResult.AllData;
        }

        private ActivitySamplingResult SampleUsingParentId(ref ActivityCreationOptions<string> options)
        {
            return ActivitySamplingResult.AllData;
        }
    }
}
