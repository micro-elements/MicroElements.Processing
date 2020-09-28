using System;
using System.Collections.Concurrent;
using NodaTime;

namespace Sberbank.Pfe2.MarketData
{
    /// <summary>
    /// Набор рыночных данных в разбивке по дням.
    /// ThreadSafe, Concurrent.
    /// </summary>
    public class MarketDataSet : IMarketDataSet
    {
        private readonly ConcurrentDictionary<LocalDate, IMarketData> _caches =
            new ConcurrentDictionary<LocalDate, IMarketData>();

        /// <inheritdoc />
        public IMarketData Get(LocalDate date)
        {
            return _caches.TryGetValue(date, out IMarketData marketData) ? marketData
               : throw new MarketDataNotFoundException(date);
        }

        /// <inheritdoc />
        public IMarketData GetOrCreate(LocalDate date, Func<LocalDate, IMarketData> factory)
        {
            return _caches.GetOrAdd(date, key => factory?.Invoke(key) ?? new MarketData(date));
        }

        /// <inheritdoc />
        public void Set(LocalDate date, IMarketData marketData)
        {
            _caches.TryAdd(date, marketData);
        }

        private class MarketDataNotFoundException : Exception
        {
            private readonly LocalDate _timestamp;

            public MarketDataNotFoundException(LocalDate timestamp) => _timestamp = timestamp;

            public override string Message => $"No market data found in cache on {_timestamp}.";
        }
    }
}
