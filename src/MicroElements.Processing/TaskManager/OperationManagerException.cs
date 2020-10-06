// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// OperationManager exception.
    /// </summary>
    [Serializable]
    public class OperationManagerException : Exception
    {
        public OperationManagerException() { }
        public OperationManagerException(string message) : base(message) { }
        public OperationManagerException(string message, Exception inner) : base(message, inner) { }
        protected OperationManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
