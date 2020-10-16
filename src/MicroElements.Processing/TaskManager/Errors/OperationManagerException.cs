// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace MicroElements.Processing.TaskManager
{
    public enum ErrorCode
    {
        /// <summary>
        /// Undefined error code.
        /// </summary>
        None,

        /// <summary>
        /// Session does not exists.
        /// </summary>
        SessionDoesNotExists,

        //$"Session is not started. SessionId: {_session.Id}"
        SessionIsNotStarted,

        //$"Session is already started. SessionId: {_session.Id}"
        SessionIsAlreadyStarted,

        /// <summary>
        /// Operation does not exists.
        /// </summary>
        OperationDoesNotExists,
    }

    /// <summary>
    /// OperationManager exception.
    /// </summary>
    [Serializable]
    public class TaskManagerException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public TaskManagerException() { }
        public TaskManagerException(string message) : base(message) { ErrorCode = ErrorCode.None; }
        public TaskManagerException(ErrorCode errorCode, string message) : base(message) { ErrorCode = errorCode;}
        public TaskManagerException(string message, Exception inner) : base(message, inner) { }
        protected TaskManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// OperationManager exception.
    /// </summary>
    [Serializable]
    public class OperationManagerException : TaskManagerException
    {
        public OperationManagerException() { }
        public OperationManagerException(string message) : base(message) { }
        public OperationManagerException(ErrorCode errorCode, string message) : base(errorCode, message) { }
        public OperationManagerException(string message, Exception inner) : base(message, inner) { }
        protected OperationManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// SessionManagerException exception.
    /// </summary>
    [Serializable]
    public class SessionManagerException : TaskManagerException
    {
        public SessionManagerException() { }
        public SessionManagerException(string message) : base(message) { }
        public SessionManagerException(ErrorCode errorCode, string message) : base(errorCode, message) { }
        public SessionManagerException(string message, Exception inner) : base(message, inner) { }
        protected SessionManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
