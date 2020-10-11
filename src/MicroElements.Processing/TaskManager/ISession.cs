// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents task that contains other tasks.
    /// </summary>
    public interface ISession : IOperation
    {
        /// <summary>
        /// Gets session messages.
        /// </summary>
        IMutableMessageList<Message> Messages { get; }

        /// <summary>
        /// Gets operations list in lazy manner because this is very expensive.
        /// </summary>
        Func<IReadOnlyCollection<IOperation>> GetOperations { get; }
    }

    /// <summary>
    /// Session with state.
    /// </summary>
    /// <typeparam name="TSessionState">Session state type.</typeparam>
    public interface ISession<out TSessionState> : ISession, IOperation<TSessionState>
    {
        /// <summary>
        /// Gets session internal state as operation.
        /// </summary>
        IOperation<TSessionState> Operation { get; }
    }

    /// <summary>
    /// Session with state and operations.
    /// </summary>
    /// <typeparam name="TSessionState">Session state type.</typeparam>
    /// <typeparam name="TOperationState">Task state type.</typeparam>
    public interface ISession<out TSessionState, out TOperationState> : ISession<TSessionState>
    {
        /// <summary>
        /// Gets operations list.
        /// </summary>
        IReadOnlyCollection<IOperation<TOperationState>> Operations { get; }
    }
}
