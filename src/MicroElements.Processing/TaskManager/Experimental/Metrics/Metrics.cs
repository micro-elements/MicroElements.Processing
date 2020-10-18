// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Processing.Metrics
{
    /// <summary>
    /// Metric types.
    /// Aligned with https://www.app-metrics.io/getting-started/metric-types/.
    /// </summary>
    public enum MetricType
    {
        Timer,
        Counter,
        Histogram,
        Meter,
        Gauge,
        Apdex
    }

    public class MetricOptions
    {
        public string Context { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IReadOnlyCollection<KeyValuePair<string, string>> Tags { get; set; }

        public MetricType MetricType { get; set; }

        public ValueUnit MeasurementUnit { get; set; }
    }

    public interface ISessionMetrics
    {
        void AddMeasure(string name, long value);
    }

    public interface IOperationMetrics
    {
        void AddMeasure(string name, long value);
    }

    public enum ValueUnit
    {
        Miliseconds
    }

    internal static class Test
    {
        public static void UsaCase()
        {
            IOperationMetrics operationMetrics = null;
            operationMetrics.AddMeasure("GlobalWait", 1200);
            operationMetrics.AddMeasure("ProcessTime", 20000);
            operationMetrics.AddMeasure("Storage", 5000);
            operationMetrics.AddMeasure("Calculator", 14000);
        }
    }
}
