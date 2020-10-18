// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Base exception that provides <see cref="IError{TErrorCode}"/>.
    /// </summary>
    /// <typeparam name="TErrorCode">Error code type.</typeparam>
    [Serializable]
    public abstract class ExceptionWithError<TErrorCode> : Exception
    {
        /// <summary>
        /// Gets Error associated with exception.
        /// </summary>
        public IError<TErrorCode> Error { get; } = TaskManager.Error.Empty<TErrorCode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionWithError{TErrorCode}"/> class.
        /// </summary>
        /// <param name="error">Error associated with exception.</param>
        public ExceptionWithError(IError<TErrorCode> error)
            : base(error.Message.FormattedMessage)
        {
            Error = error;
        }

        /// <inheritdoc />
        public ExceptionWithError()
        {
        }

        /// <inheritdoc />
        protected ExceptionWithError(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
