// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents operation status.
    /// </summary>
    public enum OperationStatus
    {
        /// <summary>
        /// Operation is created but not started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Operation is in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Operation if finished.
        /// </summary>
        Finished,

        //TODO: Failed, //можно обойтись. Успех или неудача - это трактовка результата
    }
}
