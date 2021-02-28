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
    public class SessionMetrics : IManualMetadataProvider
    {
        private readonly IMutablePropertyContainer _metadata;

        /// <inheritdoc />
        public IPropertyContainer Metadata => _metadata;

        /// <summary>
        /// Gets operations count.
        /// </summary>
        public int OperationsCount => Metadata.GetValue(SessionMetricsMeta.OperationsCount);

        /// <summary>
        /// Gets count of operations in <see cref="OperationStatus.InProgress"/> status.
        /// </summary>
        public int InProgressCount => Metadata.GetValue(SessionMetricsMeta.InProgressCount);

        /// <summary>
        /// Gets count of operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int FinishedCount => Metadata.GetValue(SessionMetricsMeta.FinishedCount);

        /// <summary>
        /// Gets count of failed operations (with <see cref="IOperation.Exception"/>) in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int ErrorCount => Metadata.GetValue(SessionMetricsMeta.ErrorCount);

        /// <summary>
        /// Gets count of success operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public int SuccessCount => Metadata.GetValue(SessionMetricsMeta.SuccessCount);

        /// <summary>
        /// Gets progress in range [0..100].
        /// </summary>
        public int ProgressInPercents => Metadata.GetValue(SessionMetricsMeta.ProgressInPercents);

        /// <summary>
        /// Gets average milliseconds per operation.
        /// </summary>
        public int AvgMillisecondsPerOperation => Metadata.GetValue(SessionMetricsMeta.AvgMillisecondsPerOperation);

        /// <summary>
        /// Gets operations per minute.
        /// </summary>
        public double OperationsPerMinute => Metadata.GetValue(SessionMetricsMeta.OperationsPerMinute);

        /// <summary>
        /// Gets operations per second.
        /// </summary>
        public double OperationsPerSecond => Metadata.GetValue(SessionMetricsMeta.OperationsPerSecond);

        /// <summary>
        /// Gets session duration.
        /// </summary>
        public Duration Duration => Metadata.GetValue(SessionMetricsMeta.Duration);

        /// <summary>
        /// Gets estimated time to finish.
        /// </summary>
        public Duration Estimation => Metadata.GetValue(SessionMetricsMeta.Estimation);

        /// <summary>
        /// Gets maximum concurrency level for session.
        /// </summary>
        public int MaxConcurrencyLevel => Metadata.GetValue(SessionMetricsMeta.MaxConcurrencyLevel);

        /// <summary>
        /// Gets Speedup Ratio. Evaluates as total time of finished operations divided by total session duration.
        /// </summary>
        public double SpeedupRatio => Metadata.GetValue(SessionMetricsMeta.SpeedupRatio);

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionMetrics"/> class.
        /// </summary>
        /// <param name="sourceValues">Values.</param>
        public SessionMetrics(IEnumerable<IPropertyValue> sourceValues)
        {
            _metadata = new MutablePropertyContainer(sourceValues);
        }
    }

    /// <summary>
    /// SessionMetrics metadata.
    /// </summary>
    public static class SessionMetricsMeta
    {
        /// <summary>
        /// Gets <see cref="IPropertySet"/> for <see cref="SessionMetrics"/>.
        /// </summary>
        public static IPropertySet PropertySet { get; }

        /// <summary>
        /// Gets properties for <see cref="SessionMetrics"/>.
        /// </summary>
        /// <returns>Properties.</returns>
        public static IEnumerable<IProperty> GetProperties()
        {
            yield return SessionMetricsMeta.GlobalConcurrencyLevel;
            yield return SessionMetricsMeta.ProcessorCount;
            yield return SessionMetricsMeta.OperationsCount;
            yield return SessionMetricsMeta.InProgressCount;
            yield return SessionMetricsMeta.FinishedCount;
            yield return SessionMetricsMeta.ErrorCount;
            yield return SessionMetricsMeta.SuccessCount;
            yield return SessionMetricsMeta.ProgressInPercents;
            yield return SessionMetricsMeta.AvgMillisecondsPerOperation;
            yield return SessionMetricsMeta.OperationsPerMinute;
            yield return SessionMetricsMeta.OperationsPerSecond;
            yield return SessionMetricsMeta.Duration;
            yield return SessionMetricsMeta.Estimation;
            yield return SessionMetricsMeta.MaxConcurrencyLevel;
            yield return SessionMetricsMeta.SpeedupRatio;
            yield return SessionMetricsMeta.TotalWaitingTime;
            yield return SessionMetricsMeta.AvgProcessingTimePerOperation;
            yield return SessionMetricsMeta.AvgWaitingTimePerOperation;
        }

        static SessionMetricsMeta()
        {
            // deal with static readonly
            PropertySet = new PropertySet(GetProperties());
        }

        /// <summary>
        /// GlobalConcurrencyLevel is max concurrency level set for all sessions in <see cref="ISessionManager"/>.
        /// </summary>
        public static readonly IProperty<int> GlobalConcurrencyLevel = new Property<int>("GlobalConcurrencyLevel")
            .WithDescription($"GlobalConcurrencyLevel is max concurrency level set for all sessions in {nameof(ISessionManager)}.");

        /// <summary>
        /// Processor count.
        /// </summary>
        public static readonly IProperty<int> ProcessorCount = new Property<int>("ProcessorCount")
            .WithDescription("Processor count.");

        /// <summary>
        /// Operations count.
        /// </summary>
        public static readonly IProperty<int> OperationsCount = new Property<int>("OperationsCount")
            .WithDescription("Operations count.");

        /// <summary>
        /// Count of operations in <see cref="OperationStatus.InProgress"/> status.
        /// </summary>
        public static readonly IProperty<int> InProgressCount = new Property<int>("InProgressCount")
            .WithDescription($"Count of operations in {nameof(OperationStatus.InProgress)} status.");

        /// <summary>
        /// Count of operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public static readonly IProperty<int> FinishedCount = new Property<int>("FinishedCount")
            .WithDescription($"Count of operations in {nameof(OperationStatus.Finished)} status.");

        /// <summary>
        /// Count of failed operations (with <see cref="IOperation.Exception"/>) in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public static readonly IProperty<int> ErrorCount = new Property<int>("ErrorCount")
            .WithDescription($"Count of failed operations with {nameof(IOperation.Exception)} in {nameof(OperationStatus.Finished)} status.");

        /// <summary>
        /// Count of success operations in <see cref="OperationStatus.Finished"/> status.
        /// </summary>
        public static readonly IProperty<int> SuccessCount = new Property<int>("SuccessCount")
            .WithDescription($"Count of success operations in {nameof(OperationStatus.Finished)} status.");

        /// <summary>
        /// Progress in range [0..100].
        /// </summary>
        public static readonly IProperty<int> ProgressInPercents = new Property<int>("ProgressInPercents")
            .WithDescription($"Progress in range [0..100].");

        /// <summary>
        /// Average milliseconds per operation.
        /// </summary>
        public static readonly IProperty<int> AvgMillisecondsPerOperation = new Property<int>("AvgMillisecondsPerOperation")
            .WithDescription($"Average milliseconds per operation.");

        /// <summary>
        /// Operations per minute.
        /// </summary>
        public static readonly IProperty<double> OperationsPerMinute = new Property<double>("OperationsPerMinute")
            .WithDescription($"Operations per minute.");

        /// <summary>
        /// Operations per second.
        /// </summary>
        public static readonly IProperty<double> OperationsPerSecond = new Property<double>("OperationsPerSecond")
            .WithDescription($"Operations per second.");

        /// <summary>
        /// Session duration.
        /// </summary>
        public static readonly IProperty<Duration> Duration = new Property<Duration>("Duration")
            .WithDescription($"Session duration.");

        /// <summary>
        /// Estimated time to finish.
        /// </summary>
        public static readonly IProperty<Duration> Estimation = new Property<Duration>("Estimation")
            .WithDescription($"Estimated time to finish.");

        /// <summary>
        /// Maximum concurrency level for session.
        /// </summary>
        public static readonly IProperty<int> MaxConcurrencyLevel = new Property<int>("MaxConcurrencyLevel")
            .WithDescription($"Maximum concurrency level for session.");

        /// <summary>
        /// Speedup Ratio. Evaluates as total execution time of finished operations divided by total session duration.
        /// </summary>
        public static readonly IProperty<double> SpeedupRatio = new Property<double>("SpeedupRatio")
            .WithDescription($"Speedup Ratio. Evaluates as total execution time of finished operations divided by total session duration.");

        /// <summary>
        /// Total waiting time sor session.
        /// </summary>
        public static readonly IProperty<Duration> TotalWaitingTime = new Property<Duration>("TotalWaitingTime")
            .WithDescription($"Total waiting time sor session.");

        /// <summary>
        /// Full processing time for operation: Wait + Execution.
        /// </summary>
        public static readonly IProperty<int> AvgProcessingTimePerOperation = new Property<int>("AvgProcessingTimePerOperation")
            .WithDescription($"Full processing time for operation: Wait + Execution.");

        /// <summary>
        /// Waiting time for global lock per operation.
        /// </summary>
        public static readonly IProperty<int> AvgWaitingTimePerOperation = new Property<int>("AvgWaitingTimePerOperation")
            .WithDescription($"Waiting time for global lock per operation.");
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
                operationsCount > 0 ? (int)(((double)finishedCount / operationsCount) * 100) : 0;

            Duration duration = session.GetDuration();
            Duration estimation = OneDay;

            int avgProcessingTimePerOperation = 0;
            int avgMillisecondsPerOperation = 0;
            double operationsPerMinute = 0;
            double operationsPerSecond = 0;
            int maxConcurrencyLevel = 0;
            double speedupRatio = 0;
            double totalWaitingTime = 0;
            int avgWaitingTimePerOperation = 0;

            if (finishedCount > 0)
            {
                if (duration.TotalMilliseconds > 0)
                {
                    operationsPerMinute = Math.Round(finishedCount / duration.TotalMinutes, 2);
                    operationsPerSecond = Math.Round(finishedCount / duration.TotalSeconds, 2);
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

                double estimationTimeInMilliseconds = duration.TotalMilliseconds * notFinished / finishedCount;
                estimation = Duration.FromMilliseconds(estimationTimeInMilliseconds);

                if (session.ExecutionOptions != null)
                {
                    maxConcurrencyLevel = session.ExecutionOptions.MaxConcurrencyLevel.GetValueOrDefault();
                }

                speedupRatio = Math.Round(totalExecutionTime / duration.TotalMilliseconds, 2);
            }

            var metrics = new MutablePropertyContainer()
                .WithValue(SessionMetricsMeta.ProcessorCount, Environment.ProcessorCount)
                .WithValue(SessionMetricsMeta.GlobalConcurrencyLevel, globalConcurrencyLevel)
                .WithValue(SessionMetricsMeta.MaxConcurrencyLevel, maxConcurrencyLevel)

                .WithValue(SessionMetricsMeta.OperationsCount, operationsCount)
                .WithValue(SessionMetricsMeta.InProgressCount, inProgressCount)
                .WithValue(SessionMetricsMeta.FinishedCount, finishedCount)
                .WithValue(SessionMetricsMeta.ErrorCount, errorCount)
                .WithValue(SessionMetricsMeta.SuccessCount, finishedCount - errorCount)
                .WithValue(SessionMetricsMeta.ProgressInPercents, progressInPercents)
                .WithValue(SessionMetricsMeta.AvgMillisecondsPerOperation, avgMillisecondsPerOperation)

                .WithValue(SessionMetricsMeta.OperationsPerMinute, operationsPerMinute)
                .WithValue(SessionMetricsMeta.OperationsPerSecond, operationsPerSecond)
                .WithValue(SessionMetricsMeta.Duration, duration)
                .WithValue(SessionMetricsMeta.Estimation, estimation)
                .WithValue(SessionMetricsMeta.SpeedupRatio, speedupRatio)

                .WithValue(SessionMetricsMeta.TotalWaitingTime, Duration.FromMilliseconds(totalWaitingTime))
                .WithValue(SessionMetricsMeta.AvgProcessingTimePerOperation, avgProcessingTimePerOperation)
                .WithValue(SessionMetricsMeta.AvgWaitingTimePerOperation, avgWaitingTimePerOperation)
                ;

            return new SessionMetrics(sourceValues: metrics);
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
