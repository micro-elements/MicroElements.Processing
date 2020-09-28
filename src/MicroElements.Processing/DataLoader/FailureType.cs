namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Вид ошибки.
    /// </summary>
    public enum FailureType
    {
        /// <summary>
        /// Нулевой результат.
        /// </summary>
        NullResult,

        /// <summary>
        /// Исключение.
        /// </summary>
        Exception,

        /// <summary>
        /// Невалидные данные из источника.
        /// </summary>
        ValidationError
    }
}