// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents task that contains other tasks.
    /// </summary>
    public interface ISession : IOperation, IEnumerable<IOperation>
    {
        /// <summary>
        /// Gets session messages.
        /// </summary>
        IMutableMessageList<Message> Messages { get; }
    }

    /// <summary>
    /// Session with state.
    /// </summary>
    /// <typeparam name="TSessionState">Session state type.</typeparam>
    public interface ISession<TSessionState> : ISession, IOperation<TSessionState>
    {
    }

    /// <summary>
    /// Session with state and operations.
    /// </summary>
    /// <typeparam name="TSessionState">Session state type.</typeparam>
    /// <typeparam name="TOperationState">Task state type.</typeparam>
    public interface ISession<TSessionState, TOperationState> : ISession<TSessionState>
    {
        /// <summary>
        /// Gets operations list.
        /// </summary>
        IReadOnlyCollection<IOperation<TOperationState>> Operations { get; }
    }
}
