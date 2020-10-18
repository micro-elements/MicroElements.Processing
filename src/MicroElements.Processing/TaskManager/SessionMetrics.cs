// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MicroElements.Metadata;
using NodaTime;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Session metrics.
    /// </summary>
    public class SessionMetrics : IMetadataProvider
    {
        private readonly IMutablePropertyContainer _metadata;

        /// <inheritdoc />
        public IPropertyContainer Metadata => _metadata;

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
        /// Gets operations per minute.
        /// </summary>
        public double OperationsPerMinute { get; }

        /// <summary>
        /// Gets operations per second.
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
        /// <param name="sourceValues">Metrics.</param>
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
            double speedupRatio,
            IEnumerable<IPropertyValue> sourceValues)
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

            _metadata = new MutablePropertyContainer(sourceValues);
        }
    }

    /// <summary>
    /// SessionMetrics metadata.
    /// </summary>
    public static class SessionMetricsMeta
    {
        /// <summary>
        /// GlobalConcurrencyLevel is max concurrency level set for all sessions in <see cref="ISessionManager"/>.
        /// </summary>
        public static readonly IProperty<int> GlobalConcurrencyLevel = new Property<int>("GlobalConcurrencyLevel");

        /// <summary>
        /// Processor count.
        /// </summary>
        public static readonly IProperty<int> ProcessorCount = new Property<int>("ProcessorCount");

        /// <summary>
        /// Gets operations count.
        /// </summary>
        public static readonly IProperty<int> OperationsCount = new Property<int>("OperationsCount");

        /// <summary>
        /// Gets count of operations in <see cref="OperationStatus.InProgress"/> status.
        /// </summary>
        public static readonly IProperty<int> InProgressCount = new Property<int>("InProgressCount");

        /// <summary>
        /// Gets count of operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public static readonly IProperty<int> FinishedCount = new Property<int>("FinishedCount");

        /// <summary>
        /// Gets count of failed operations (with <see cref="IOperation.Exception"/>) in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public static readonly IProperty<int> ErrorCount = new Property<int>("ErrorCount");

        /// <summary>
        /// Gets count of success operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public static readonly IProperty<int> SuccessCount = new Property<int>("SuccessCount");

        /// <summary>
        /// Gets progress in range [0..100].
        /// </summary>
        public static readonly IProperty<int> ProgressInPercents = new Property<int>("ProgressInPercents");

        /// <summary>
        /// Gets average milliseconds per operation.
        /// </summary>
        public static readonly IProperty<int> AvgMillisecondsPerOperation = new Property<int>("AvgMillisecondsPerOperation");

        /// <summary>
        /// Gets operations per minute.
        /// </summary>
        public static readonly IProperty<double> OperationsPerMinute = new Property<double>("OperationsPerMinute");

        /// <summary>
        /// Gets operations per second.
        /// </summary>
        public static readonly IProperty<double> OperationsPerSecond = new Property<double>("OperationsPerSecond");

        /// <summary>
        /// Gets session duration.
        /// </summary>
        public static readonly IProperty<Duration> Duration = new Property<Duration>("Duration");

        /// <summary>
        /// Gets estimated time to finish.
        /// </summary>
        public static readonly IProperty<Duration> Estimation = new Property<Duration>("Estimation");

        /// <summary>
        /// Gets maximum concurrency level for session.
        /// </summary>
        public static readonly IProperty<int> MaxConcurrencyLevel = new Property<int>("MaxConcurrencyLevel");

        /// <summary>
        /// Gets Speedup Ratio. Evaluates as total execution time of finished operations divided by total session duration.
        /// </summary>
        public static readonly IProperty<double> SpeedupRatio = new Property<double>("SpeedupRatio");

        /// <summary>
        /// Total waiting time sor session.
        /// </summary>
        public static readonly IProperty<Duration> TotalWaitingTime = new Property<Duration>("TotalWaitingTime");

        /// <summary>
        /// Full processing time for operation: Wait + Execution.
        /// </summary>
        public static readonly IProperty<int> AvgProcessingTimePerOperation = new Property<int>("AvgProcessingTimePerOperation");

        /// <summary>
        /// Gets waiting time for global lock per operation.
        /// </summary>
        public static readonly IProperty<int> AvgWaitingTimePerOperation = new Property<int>("AvgWaitingTimePerOperation");
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

            int globalConcurrencyLevel = session.Metadata.GetValue(SessionMetricsMeta.GlobalConcurrencyLevel);

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

            int avgProcessingTimePerOperation = 0;
            int avgMillisecondsPerOperation = 0;
            double operationsPerMinute = 0;
            double operationsPerSecond = 0;
            int concurrencyLevel = 0;
            double speedupRatio = 0;
            double totalWaitingTime = 0;
            int avgWaitingTimePerOperation = 0;

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

                totalWaitingTime = operations
                    .Where(operation => operation.Status == OperationStatus.Finished)
                    .Aggregate(0.0, (ms, operation) => ms + operation.Metadata.GetValue(OperationMeta.GlobalWaitDuration).TotalMilliseconds);

                double totalExecutionTime = totalFinishedDuration - totalWaitingTime;

                avgProcessingTimePerOperation = (int)(totalFinishedDuration / finishedCount);
                avgMillisecondsPerOperation = (int)(totalExecutionTime / finishedCount);
                avgWaitingTimePerOperation = avgProcessingTimePerOperation / finishedCount;

                double estimationTimeInMilliseconds = sessionDuration.TotalMilliseconds * notFinished / finishedCount;
                estimation = Duration.FromMilliseconds(estimationTimeInMilliseconds);

                if (session.ExecutionOptions != null)
                {
                    concurrencyLevel = session.ExecutionOptions.MaxConcurrencyLevel;
                }

                speedupRatio = Math.Round(totalExecutionTime / sessionDuration.TotalMilliseconds, 2);
            }

            var metrics = new MutablePropertyContainer()
                .WithValue(SessionMetricsMeta.ProcessorCount, Environment.ProcessorCount)
                .WithValue(SessionMetricsMeta.GlobalConcurrencyLevel, globalConcurrencyLevel)
                .WithValue(SessionMetricsMeta.MaxConcurrencyLevel, concurrencyLevel)

                .WithValue(SessionMetricsMeta.OperationsCount, operationsCount)
                .WithValue(SessionMetricsMeta.InProgressCount, inProgressCount)
                .WithValue(SessionMetricsMeta.FinishedCount, finishedCount)
                .WithValue(SessionMetricsMeta.ErrorCount, errorCount)
                .WithValue(SessionMetricsMeta.SuccessCount, finishedCount - errorCount)
                .WithValue(SessionMetricsMeta.ProgressInPercents, progressInPercents)
                .WithValue(SessionMetricsMeta.AvgMillisecondsPerOperation, avgMillisecondsPerOperation)

                .WithValue(SessionMetricsMeta.OperationsPerMinute, operationsPerMinute)
                .WithValue(SessionMetricsMeta.OperationsPerSecond, operationsPerSecond)
                .WithValue(SessionMetricsMeta.Duration, sessionDuration)
                .WithValue(SessionMetricsMeta.Estimation, estimation)
                .WithValue(SessionMetricsMeta.SpeedupRatio, speedupRatio)

                .WithValue(SessionMetricsMeta.TotalWaitingTime, Duration.FromMilliseconds(totalWaitingTime))
                .WithValue(SessionMetricsMeta.AvgProcessingTimePerOperation, avgProcessingTimePerOperation)
                .WithValue(SessionMetricsMeta.AvgWaitingTimePerOperation, avgWaitingTimePerOperation)
                ;

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
                speedupRatio: speedupRatio,
                sourceValues: metrics);
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
