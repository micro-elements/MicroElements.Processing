using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Sberbank.Pfe2.Integration.MarketData.Loader;

namespace Sberbank.Pfe2.Domain.Model
{
    /// <summary>
    /// Тип данных, который знает свой источник.
    /// Часто требуется получить данные из одного источника, если не получилось, то из другого и т.д.
    /// Данный тип в дальнейшем можно расширить таймингами или еще чем.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [ImmutableObject(true)]
    public class DataSource<TData>
    {
        /// <summary>
        /// DataSource with no value.
        /// </summary>
        public static readonly DataSource<TData> NoValue = new DataSource<TData>(default(TData), SourceType.NoValue, DateTime.MinValue);

        public static readonly DataSource<TData> BadMarketData = new DataSource<TData>(default(TData), SourceType.BadMarketData, DateTime.MinValue);

        /// <summary>
        /// Значение.
        /// </summary>
        public TData Value { get; }

        /// <summary>
        /// Тип источника данных.
        /// </summary>
        public SourceType Type { get; }

        /// <summary>
        /// Ключ ресурса.
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// Дата ресурса.
        /// </summary>
        public DateTimeOffset Date { get; }

        /// <summary>
        /// Создание DataSource.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="type">Тип источника.</param>
        /// <param name="key">Ключ ресурса.</param>
        /// <param name="date">Дата ресурса.</param>
        public DataSource(TData value, SourceType type, object key = null, DateTimeOffset? date = null)
        {
            Value = value;
            Type = type;
            Key = key;
            Date = date ?? DateTimeOffset.Now;
        }

        /// <summary>
        /// Конвертация в тип, который оборачивает DataSource.
        /// </summary>
        public static implicit operator TData(DataSource<TData> dataSource)
        {
            return dataSource.Value;
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(Value)}: {Value}, {nameof(Type)}: {Type}";
    }

    public static class DataSourceExtensions
    {
        public static DataSource<TData> ToDataSource<TData>([NotNull]this TData value, SourceType type, object key = null, DateTime? date = null) where TData : class
        {
            return new DataSource<TData>(value, type, key, date);
        }

        public static DataSource<TData> ToDataSourceOrNoValue<TData>([CanBeNull]this TData value, SourceType type, object key = null, DateTime? date = null) where TData : class
        {
            return value == default(TData) ? DataSource<TData>.NoValue : ToDataSource(value, type, key, date);
        }

        public static DataSource<TData> EnsureFilled<TData>([NotNull]this DataSource<TData> dataSource, object key, DateTime? date = null)
        {
            if(dataSource.Key == null )
                return new DataSource<TData>(dataSource.Value, dataSource.Type, key, dataSource.Date);
            return dataSource;
        }

        //public static Task<TData> ToTask<TData>(this TData data)
        //{
        //    return Task.FromResult(data);
        //}

        public static bool HasValue<TData>(this DataSource<TData> dataSource)
        {
            return dataSource != null && dataSource.Type != SourceType.NoValue;
        }

        public static DataSource<TValue> TryGet<TKey, TValue>(this IDictionary<TKey, DataSource<TValue>> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;
            return DataSource<TValue>.NoValue;
        }
    }
}
