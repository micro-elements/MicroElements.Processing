using System;
using JetBrains.Annotations;
using NodaTime;

namespace Sberbank.Pfe2.MarketData
{
    /// <summary>
    /// Набор рыночных данных в разбивке по дням.
    /// </summary>
    public interface IMarketDataSet
    {
        /// <summary>
        /// Получение рыночных данных за определенную дату.
        /// Если данных нет, то используется фабричный метод.
        /// </summary>
        /// <param name="date">Дата рыночных данных.</param>
        /// <returns>Рыночные данные.</returns>
        [NotNull] IMarketData Get(LocalDate date);

        /// <summary>
        /// Получение рыночных данных за определенную дату.
        /// Если данных нет, то используется фабричный метод.
        /// </summary>
        /// <param name="date">Дата рыночных данных.</param>
        /// <param name="factory">Фабрика создания экземпляра <see cref="IMarketData"/>.</param>
        /// <returns>Рыночные данные.</returns>
        [NotNull] IMarketData GetOrCreate(LocalDate date, [CanBeNull] Func<LocalDate, IMarketData> factory);

        /// <summary>
        /// Установка рыночных данных на определенный день.
        /// </summary>
        /// <param name="date">Дата рыночных данных.</param>
        /// <param name="marketData">Рыночные данные.</param>
        void Set(LocalDate date, IMarketData marketData);
    }
}
