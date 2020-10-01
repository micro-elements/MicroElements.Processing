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
        /// Single session time to live in cache.
        /// Format: [-][d.]hh:mm:ss.
        /// </summary>
        TimeSpan SessionCacheTimeToLive { get; }
    }

    /// <summary>
    /// SessionManager configuration.
    /// </summary>
    public class SessionManagerConfiguration : ISessionManagerConfiguration
    {
        /// <inheritdoc />
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public int MaxConcurrencyLevel { get; set; } = Environment.ProcessorCount;

        /// <inheritdoc />
        public TimeSpan SessionCacheTimeToLive { get; set; } = TimeSpan.FromDays(2);
    }
}
