using System;

namespace Sberbank.Pfe2.MarketData
{
    /// <summary>
    /// Configuration for MarketDataService.
    /// </summary>
    public interface IMarketDataConfiguration
    {
        /// <summary>
        /// MDS Url.
        /// </summary>
        Uri ServiceUrl { get; }

        /// <summary>
        /// Request prefix.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Timeout to MDS in milliseconds.
        /// </summary>
        int RequestTimeOut { get; }

        /// <summary>
        /// Max threads for loading.
        /// </summary>
        byte ConcurrencyLimit { get; }
    }
}
