// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Provides error message.
    /// </summary>
    public interface IError
    {
        /// <summary>
        /// Gets error message.
        /// </summary>
        public Message Message { get; }
    }

    /// <summary>
    /// Provides error information.
    /// </summary>
    public interface IError<TErrorCode> : IError
    {
        /// <summary>
        /// Gets known error code.
        /// </summary>
        public TErrorCode ErrorCode { get; }
    }
}
