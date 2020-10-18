// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// OpenTelemetry stuff.
    /// </summary>
    public static class OpenTelemetry
    {
        public static readonly ActivitySource Processing = new ActivitySource(
            name: "MicroElements.Processing");
    }
}
