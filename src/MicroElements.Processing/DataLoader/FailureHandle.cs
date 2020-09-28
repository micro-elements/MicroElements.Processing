namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Способ обработки ошибки.
    /// </summary>
    public enum FailureHandle
    {
        /// <summary>
        /// Перейти к следующему шагу, если есть.
        /// </summary>
        Continue,

        /// <summary>
        /// Залогировать ошибку в виде информационного собщения.
        /// Перейти к следующему шагу.
        /// </summary>
        LogAndContinue,

        /// <summary>
        /// Залогировать ошибку в виде warning собщения.
        /// Перейти к следующему шагу.
        /// </summary>
        LogWarnAndContinue,

        /// <summary>
        /// Выкинуть исключение.
        /// Если исключение есть, то они заворачивается в <see cref="FailureException"/>. Если пустой результат, то просто <see cref="FailureException"/>.
        /// </summary>
        Throw,

        /// <summary>
        /// Цепочка шагов прерывается. В список сообщений добавляется сообщение с типом <see cref="MessageSeverity.Error"/>.
        /// </summary>
        BreakWithError
    }
}
