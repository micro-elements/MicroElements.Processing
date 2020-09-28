using Polly;

namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Контекст загрузки данных.
    /// </summary>
    internal class MarketDataContext
    {
        public MarketDataRequest MarketDataRequest { get; }
        public IMarketData MarketData { get; }
        public bool ParallelExecution { get; }
        public IAsyncPolicy ExecutionPolicy { get; }

        public MarketDataContext(
            MarketDataRequest marketDataRequest,
            IMarketData marketData,
            bool parallelExecution,
            IAsyncPolicy executionPolicy)
        {
            MarketDataRequest = marketDataRequest;
            MarketData = marketData;
            ParallelExecution = parallelExecution;
            ExecutionPolicy = executionPolicy;
        }
    }
}
