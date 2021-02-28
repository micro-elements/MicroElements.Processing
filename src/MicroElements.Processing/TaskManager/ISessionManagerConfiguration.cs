// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// SessionManager configuration.
    /// </summary>
    public interface ISessionManagerConfiguration
    {
        /// <summary>
        /// Gets session manager id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets global maximum concurrency level for all sessions.
        /// </summary>
        int MaxConcurrencyLevel { get; }

        /// <summary>
        /// Gets session time to live in cache (for one session).
        /// Format: [-][d.]hh:mm:ss.
        /// </summary>
        TimeSpan SessionCacheTimeToLive { get; }
    }

    /// <summary>
    /// SessionManager configuration.
    /// </summary>
    public class SessionManagerConfiguration : ISessionManagerConfiguration
    {
        /// <summary>
        /// Creates new configuration instance with default values filled.
        /// </summary>
        /// <returns>new configuration instance.</returns>
        public static SessionManagerConfiguration New()
        {
            return new SessionManagerConfiguration()
            {
                Id = Guid.NewGuid().ToString(),
                MaxConcurrencyLevel = Environment.ProcessorCount,
                SessionCacheTimeToLive = TimeSpan.FromDays(2),
            };
        }

        /// <inheritdoc />
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public int MaxConcurrencyLevel { get; set; } = Environment.ProcessorCount;

        /// <inheritdoc />
        public TimeSpan SessionCacheTimeToLive { get; set; } = TimeSpan.FromDays(2);
    }
}
