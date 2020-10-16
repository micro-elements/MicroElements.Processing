// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Session metrics.
    /// </summary>
    public class SessionMetrics
    {
        /// <summary>
        /// Gets operations count.
        /// </summary>
        public int OperationsCount { get; }

        /// <summary>
        /// Gets count of operations in <see cref="OperationStatus.InProgress"/> status.
        /// </summary>
        public int InProgressCount { get; }

        /// <summary>
        /// Gets count of operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int FinishedCount { get; }

        /// <summary>
        /// Gets count of failed operations (with <see cref="IOperation.Exception"/>) in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int ErrorCount { get; }

        /// <summary>
        /// Gets count of success operations in <see cref="OperationStatus.Finished"/> status.
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
        /// Gets operations per minute metric.
        /// </summary>
        public double OperationsPerMinute { get; }

        /// <summary>
        /// Gets operations per second metric.
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

        /// <summary>
        /// Gets maximum concurrency level for session.
        /// </summary>
        public int MaxConcurrencyLevel { get; }

        /// <summary>
        /// Gets Speedup Ratio. Evaluates as total time of finished operations divided by total session duration.
        /// </summary>
        public double SpeedupRatio { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionMetrics"/> class.
        /// </summary>
        /// <param name="operationsCount">Operations count.</param>
        /// <param name="inProgressCount">Count of operations in <see cref="OperationStatus.InProgress"/> status.</param>
        /// <param name="finishedCount">Count of finished operations.</param>
        /// <param name="errorCount">Count of failed operations.</param>
        /// <param name="progressInPercents">Progress in range [0..100].</param>
        /// <param name="avgMillisecondsPerOperation">Average milliseconds per operation.</param>
        /// <param name="operationsPerMinute">Operations per minute metric.</param>
        /// <param name="operationsPerSecond">Operations per second metric.</param>
        /// <param name="duration">Session duration.</param>
        /// <param name="estimation">Estimated time to finish.</param>
        /// <param name="maxConcurrencyLevel">Maximum concurrency level for session.</param>
        /// <param name="speedupRatio">Speedup Ratio.</param>
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
            Duration estimation,
            int maxConcurrencyLevel,
            double speedupRatio)
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
            MaxConcurrencyLevel = maxConcurrencyLevel;
            SpeedupRatio = speedupRatio;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="ISession{TSessionState}"/>.
    /// </summary>
    public static class SessionMetricsExtensions
    {
        private static readonly Duration OneDay = Duration.FromDays(1).Minus(Duration.FromSeconds(1));

        /// <summary>
        /// Calculates session metrics.
        /// </summary>
        /// <typeparam name="TSessionState">Session state.</typeparam>
        /// <param name="session">Source session.</param>
        /// <returns><see cref="SessionMetrics"/> instance.</returns>
        public static SessionMetrics GetMetrics<TSessionState>(this ISession<TSessionState> session)
        {
            var operations = session.GetOperations();

            int operationsCount = operations.Count;
            int inProgressCount = operations.Count(operation => operation.Status == OperationStatus.InProgress);
            int finishedCount = operations.Count(operation => operation.Status == OperationStatus.Finished);
            int errorCount = operations.Count(operation => operation.Exception != null);
            int notFinished = operationsCount - finishedCount;

            int progressInPercents =
                session.Status == OperationStatus.NotStarted ? 0 :
                session.Status == OperationStatus.Finished ? 100 :
                operationsCount > 0 ? (int)(((double)finishedCount / (double)operationsCount) * 100) : 0;

            Duration sessionDuration = session.GetDuration();
            Duration estimation = OneDay;

            int avgMillisecondsPerOperation = 0;
            double operationsPerMinute = 0;
            double operationsPerSecond = 0;

            int concurrencyLevel = 0;
            double speedupRatio = 0;

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

                double estimationTimeInMilliseconds = sessionDuration.TotalMilliseconds * notFinished / finishedCount;
                estimation = Duration.FromMilliseconds(estimationTimeInMilliseconds);

                if (session.ExecutionOptions != null)
                {
                    concurrencyLevel = session.ExecutionOptions.MaxConcurrencyLevel;
                }
                speedupRatio = Math.Round(totalFinishedDuration / sessionDuration.TotalMilliseconds, 2);
            }

            // TODO: to add
            int processorCount = Environment.ProcessorCount;
            int globalConcurrencyLevel;
            // TODO: GlobalLock waiting time
            // TODO: Operation metrics? ServerTiming?
            // TODO: ToContainer


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
                estimation: estimation,
                maxConcurrencyLevel: concurrencyLevel,
                speedupRatio: speedupRatio);
        }

        /// <summary>
        /// Gets CPU usage for process.
        /// Source: https://medium.com/@jackwild/getting-cpu-usage-in-net-core-7ef825831b8b.
        /// </summary>
        /// <returns>CPU usage in range [0..100].</returns>
        public static async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
    }
}
