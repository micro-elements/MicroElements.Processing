// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Session metrics.
    /// </summary>
    public class SessionMetrics
    {
        /// <summary>
        /// Operations count.
        /// </summary>
        public int OperationsCount { get; }

        /// <summary>
        /// Count of operations in <see cref="OperationStatus.InProgress"/> status.
        /// </summary>
        public int InProgressCount { get; }

        /// <summary>
        /// Count of operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int FinishedCount { get; }

        /// <summary>
        /// Count of failed operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int ErrorCount { get; }

        /// <summary>
        /// Count of success operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int SuccessCount => FinishedCount - ErrorCount;

        /// <summary>
        /// Gets progress in range [0..100].
        /// </summary>
        public int ProgressInPercents { get; }

        /// <summary>
        /// Gets average milliseconds per operation.
        /// </summary>
        public int AvgMillisecondsPerOperation { get; }

        /// <summary>
        /// Gets operation per minute metric.
        /// </summary>
        public double OperationsPerMinute { get; }

        /// <summary>
        /// Gets operation per second metric.
        /// </summary>
        public double OperationsPerSecond { get; }

        /// <summary>
        /// Gets session duration.
        /// </summary>
        public Duration Duration { get; }

        /// <summary>
        /// Gets estimated time to finish.
        /// </summary>
        public Duration Estimation { get; }

        public SessionMetrics(
            int operationsCount,
            int inProgressCount,
            int finishedCount,
            int errorCount,
            int progressInPercents,
            int avgMillisecondsPerOperation,
            double operationsPerMinute,
            double operationsPerSecond,
            Duration duration,
            Duration estimation)
        {
            OperationsCount = operationsCount;
            InProgressCount = inProgressCount;
            FinishedCount = finishedCount;
            ErrorCount = errorCount;
            ProgressInPercents = progressInPercents;
            AvgMillisecondsPerOperation = avgMillisecondsPerOperation;
            OperationsPerMinute = operationsPerMinute;
            OperationsPerSecond = operationsPerSecond;
            Duration = duration;
            Estimation = estimation;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="ISession{TSessionState}"/>.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Gets session metrics.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <typeparam name="TOperationState">Operation state.</typeparam>
        /// <param name="session">Source session.</param>
        /// <returns><see cref="SessionMetrics"/> instance.</returns>
        public static SessionMetrics GetMetrics<TSessionState, TOperationState>(this ISession<TSessionState, TOperationState> session)
        {
            IReadOnlyCollection<IOperation<TOperationState>> operations = session.Operations;

            int operationsCount = operations.Count;
            int inProgressCount = operations.Count(operation => operation.Status == OperationStatus.InProgress);
            int finishedCount = operations.Count(operation => operation.Status == OperationStatus.Finished);
            int errorCount = operations.Count(operation => operation.Exception != null);
            int progressInPercents =
                session.Status == OperationStatus.NotStarted ? 0 :
                session.Status == OperationStatus.Finished ? 100 :
                operationsCount > 0 ? (int)(((double)finishedCount / (double)operationsCount) * 100) : 0;

            Duration sessionDuration = session.GetDuration();
            Duration estimation = Duration.FromDays(1).Minus(Duration.FromSeconds(1));

            int avgMillisecondsPerOperation = 0;
            double operationsPerMinute = 0;
            double operationsPerSecond = 0;

            if (finishedCount > 0)
            {
                if (sessionDuration.TotalMilliseconds > 0)
                {
                    operationsPerMinute = Math.Round(finishedCount / sessionDuration.TotalMinutes, 2);
                    operationsPerSecond = Math.Round(finishedCount / sessionDuration.TotalSeconds, 2);
                }

                double totalFinishedDuration = operations
                    .Where(operation => operation.Status == OperationStatus.Finished)
                    .Aggregate(0.0, (ms, operation) => ms + operation.GetDuration().TotalMilliseconds);
                avgMillisecondsPerOperation = (int)(totalFinishedDuration / finishedCount);

                int notFinished = operationsCount - finishedCount;
                double estimationTimeInMilliseconds = sessionDuration.TotalMilliseconds * notFinished / finishedCount;
                estimation = Duration.FromMilliseconds(estimationTimeInMilliseconds);
            }

            return new SessionMetrics(
                operationsCount: operationsCount,
                inProgressCount: inProgressCount,
                finishedCount: finishedCount,
                errorCount: errorCount,
                progressInPercents: progressInPercents,
                avgMillisecondsPerOperation: avgMillisecondsPerOperation,
                operationsPerMinute: operationsPerMinute,
                operationsPerSecond: operationsPerSecond,
                duration: sessionDuration,
                estimation: estimation);
        }
    }
}
