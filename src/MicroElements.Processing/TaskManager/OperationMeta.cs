// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Metadata available for <see cref="IOperation"/>.
    /// </summary>
    public static class OperationMeta
    {
        /// <summary>
        /// Duration operation waited on global lock.
        /// </summary>
        public static readonly IProperty<TimeSpan> GlobalWaitDuration = new Property<TimeSpan>("GlobalWaitDuration")
            .WithDescription("Duration operation waited on global lock.");
    }
}
