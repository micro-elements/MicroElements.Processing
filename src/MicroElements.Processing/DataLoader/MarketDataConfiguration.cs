using System;

namespace Sberbank.Pfe2.MarketData
{
    /// <summary>
    /// Configuration for MarketDataService.
    /// </summary>
    public class MarketDataConfiguration : IMarketDataConfiguration
    {
        public Uri ServiceUrl { get; set; }
        public string Source { get; set; } = "MUREX_MARKET_DATA";
        public int RequestTimeOut { get; set; } = 120000;
        public byte ConcurrencyLimit { get; set; } = 4;
        public int ProbingDaysCount { get; set; } = 1;
        public bool UseMappingsFromRepository { get; set; } = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MarketDataConfiguration() { }

        /// <summary>
        /// Копирующий конструктор.
        /// </summary>
        public MarketDataConfiguration(IMarketDataConfiguration configuration)
        {
            ServiceUrl = configuration.ServiceUrl;
            Source = configuration.Source;
            RequestTimeOut = configuration.RequestTimeOut;
            ConcurrencyLimit = configuration.ConcurrencyLimit;
        }
    }
}
