using System.Collections.Concurrent;
using System.Collections.Generic;
using MicroElements.Functional;
using NodaTime;
using Sberbank.Pfe2.Domain.Model;

namespace Sberbank.Pfe2.MarketData
{
    /// <summary>
    /// Набор рыночных данных на определенную дату.
    /// </summary>
    public class MarketData : IMarketData
    {
        /// <summary>
        /// Рыночные данные.
        /// </summary>
        /// <param name="date">Дата рыночных данных.</param>
        public MarketData(LocalDate date)
        {
            Date = date;
            Messages = new ConcurrentMessageList<Message>();
            _results = new ConcurrentDictionary<string, ResultGroup>();
        }

        /// <inheritdoc />
        public LocalDate Date { get; }

        /// <inheritdoc />
        public ConcurrentDictionary<string, string> Properties { get; } = new ConcurrentDictionary<string, string>();

        public IMutableMessageList<Message> Messages { get; }

        private ResultGroup GetResultGroup<TKey, TData>(IDictionary<TKey, DataSource<TData>> dictionary, string name)
        {
            return new ResultGroup(name, typeof(TKey), typeof(TData), dictionary);
        }

        private readonly ConcurrentDictionary<string, ResultGroup> _results;

        public ConcurrentDictionary<TKey, DataSource<TResult>> GetResults<TKey, TResult>(string sectionName)
        {
            ResultGroup ValueFactory(string name)
            {
                return new ResultGroup(name, typeof(TKey), typeof(TResult), new ConcurrentDictionary<TKey, DataSource<TResult>>());
            }

            return (ConcurrentDictionary<TKey, DataSource<TResult>>)_results.GetOrAdd(sectionName, ValueFactory).Dictionary;
        }

        /// <inheritdoc />
        public IEnumerable<ResultGroup> GetResultGroups()
        {
            return _results.Values;
        }

        /// <inheritdoc />
        public bool TryAddResult<TKey, TResult>(string sectionName, TKey key, DataSource<TResult> result)
        {
            var results = GetResults<TKey, TResult>(sectionName);
            return results.TryAdd(key, result);
        }

        /// <inheritdoc />
        public DataSource<TResult> GetResult<TKey, TResult>(string sectionName, TKey key)
        {
            var results = GetResults<TKey, TResult>(sectionName);
            if (results.TryGetValue(key, out var result))
                return result;
            return DataSource<TResult>.NoValue;
        }
    }

    public class MarketData<TState> : MarketData
    {
        public TState State { get; }

        /// <inheritdoc />
        public MarketData(LocalDate date, TState state) : base(date)
        {
            State = state;
        }
    }
}
