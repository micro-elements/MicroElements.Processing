using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Sberbank.Pfe2.Domain.Model;

namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Запрос на получение данных.
    /// </summary>
    public class MarketDataRequest
    {
        /// <summary>
        /// Дата на которую грузятся данные.
        /// </summary>
        public LocalDate AsOfDate { get; set; }

        #region Данные, которые участвуют в загрузке, но не типичны, если писать обобщенную реализацию 

        private LocalDate? _marketDataDate;

        /// <summary>
        /// Дата на которую грузятся рыночные данные.
        /// Может отличаться от <see cref="AsOfDate"/> так как MarketData не содержит данные за текущую дату.
        /// </summary>
        public LocalDate MarketDataDate
        {
            get => _marketDataDate.GetValueOrDefault(AsOfDate);
            set => _marketDataDate = value;
        }

        public double Basis { get; set; } = 365.0;
        public IEnumerable<DateTime> Timeline { get; set; }

        #endregion

        #region Обобщенная реализация загрузки

        /// <summary>
        /// Динамический список ключей на загрузку данных.
        /// </summary>
        public List<object> DataKeys { get; set; } = new List<object>();

        /// <summary>
        /// Список запросов на получение данных.
        /// </summary>
        public List<IDataRequest> DataRequests { get; } = new List<IDataRequest>();

        #endregion

        #region Настройки загрузки

        /// <summary>
        /// Источники, которые нужно грузить. Если не указано, то все возможные.
        /// </summary>
        public IReadOnlyList<string> SourcesToLoad { get; set; }

        /// <summary>
        /// Источники, которые нужно исключить. Если не указано, то ничего не исключается.
        /// </summary>
        public IReadOnlyList<string> SourcesToSkip { get; set; }

        /// <summary>
        /// Степень параллелизации.
        /// </summary>
        public int ConcurrencyLimit { get; set; } = 4;

        /// <summary>
        /// Настройка state машины по загрузке. Что далаем, если произошла ошибка получения данных не на последнем шаге.
        /// </summary>
        public FailureHandle DefaultFailureHandleForNotLastStep { get; set; } = FailureHandle.LogAndContinue;

        /// <summary>
        /// Настройка state машины по загрузке. Что далаем, если произошла ошибка получения данных на последнем шаге.
        /// </summary>
        public FailureHandle DefaultFailureHandleForLastStep { get; set; } = FailureHandle.BreakWithError;

        #endregion

        #region Methods

        public IReadOnlyList<TKey> GetKeys<TKey>()
        {
            return DataKeys.Where(key => key is TKey).Cast<TKey>().ToList();
        }

        public MarketDataRequest WithDataKeys<TKey>(IReadOnlyCollection<TKey> keys)
        {
            DataKeys.AddRange(keys.Cast<object>());
            return this;
        }

        #endregion
    }

    public interface IDataRequest
    {
        Type KeyType { get; }
        Type ResultType { get; }
    }

    public class ResultContext<TResult, TKey>
    {
        public LocalDate DateTime { get; }
        public TKey Key { get; }
        public DataSource<TResult> Result { get; }

        public ResultContext(LocalDate dateTime, TKey key, DataSource<TResult> result)
        {
            DateTime = dateTime;
            Key = key;
            Result = result;
        }
    }

    public class DataRequest<TResult, TKey> : IDataRequest
    {
        public string Name { get; set; }
        public List<TKey> Keys { get; set; }
        public GetValueStep<TResult, LocalDate, TKey>[] GetValueSteps { get; set; }
        public Action<ResultContext<TResult, TKey>> OnResult { get; set; }

        /// <inheritdoc />
        public Type KeyType => typeof(TKey);

        /// <inheritdoc />
        public Type ResultType => typeof(TResult);
    }

}
