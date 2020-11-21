// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;
using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// OperationManager exception.
    /// </summary>
    [Serializable]
    public class OperationManagerException : ExceptionWithError<ErrorCode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationManagerException"/> class.
        /// </summary>
        /// <param name="error">Error associated with exception.</param>
        public OperationManagerException(Error<ErrorCode> error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationManagerException"/> class.
        /// </summary>
        public OperationManagerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationManagerException"/> class.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        protected OperationManagerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
