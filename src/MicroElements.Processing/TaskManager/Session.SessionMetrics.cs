// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

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
        public int ProgressInPercents => OperationsCount > 0 ? (FinishedCount / OperationsCount) * 100 : 0;

        public SessionMetrics(
            int operationsCount,
            int inProgressCount,
            int finishedCount,
            int errorCount)
        {
            OperationsCount = operationsCount;
            InProgressCount = inProgressCount;
            FinishedCount = finishedCount;
            ErrorCount = errorCount;
        }
    }

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
            var operations = session.Operations;

            int operationsCount = operations.Count;
            int inProgressCount = operations.Count(operation => operation.Status == OperationStatus.InProgress);
            int finishedCount = operations.Count(operation => operation.Status == OperationStatus.Finished);
            int errorCount = operations.Count(operation => operation.Exception != null);

            return new SessionMetrics(
                operationsCount,
                inProgressCount,
                finishedCount,
                errorCount);
        }
    }
}
