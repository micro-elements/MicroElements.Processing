// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;
using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// SessionManagerException exception.
    /// </summary>
    [Serializable]
    public class SessionManagerException : ExceptionWithError<ErrorCode>
    {
        /// <inheritdoc />
        public SessionManagerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManagerException"/> class.
        /// </summary>
        /// <param name="error">Error associated with exception.</param>
        public SessionManagerException(Error<ErrorCode> error)
            : base(error)
        {
        }

        /// <inheritdoc />
        protected SessionManagerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
