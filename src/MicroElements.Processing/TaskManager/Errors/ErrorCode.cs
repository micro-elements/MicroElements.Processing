// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Error codes.
    /// </summary>
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

        /// <summary>
        /// Session is not started.
        /// </summary>
        SessionIsNotStarted,

        /// <summary>
        /// Session is already started.
        /// </summary>
        SessionIsAlreadyStarted,

        /// <summary>
        /// Session update is prohibited.
        /// </summary>
        SessionUpdateIsProhibited,

        /// <summary>
        /// Operation does not exists.
        /// </summary>
        OperationDoesNotExists,

        /// <summary>
        /// Provided OperationId does not match existing OperationId.
        /// </summary>
        OperationIdDoesNotMatch,
    }
}
