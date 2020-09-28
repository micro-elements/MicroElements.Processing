using System;

namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Контекст ошибки.
    /// </summary>
    public class FailureContext
    {
        /// <summary>
        /// Создание контекста ошибки.
        /// </summary>
        /// <param name="failureType">Тип ошибки.</param>
        /// <param name="exception">Исключение (опционально).</param>
        public FailureContext(FailureType failureType, Exception exception = null)
        {
            FailureType = failureType;
            Exception = exception;
        }

        /// <summary>
        /// Вид ошибки.
        /// </summary>
        public FailureType FailureType { get; }

        /// <summary>
        /// Исключение.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Способ обработки ошибки.
        /// </summary>
        public FailureHandle OnFailure { get; set; } = FailureHandle.LogAndContinue;

        /// <summary>
        /// Установка способа обработки ошибки.
        /// </summary>
        /// <param name="failureHandle">Способ обработки ошибки.</param>
        /// <returns>Текущий контекст для возможности комбинации вызовов.</returns>
        public FailureContext SetOnFailure(FailureHandle failureHandle)
        {
            OnFailure = failureHandle;
            return this;
        }

        /// <summary>
        /// Форматирование сообщения.
        /// </summary>
        /// <param name="failure">Контекст ошибки.</param>
        /// <returns>Полное сообщение.</returns>
        public static string FormatMessage(FailureContext failure)
        {
            return failure != null
                ? failure.Exception != null ? $"{failure.FailureType}: {GetFullExceptionMessage(failure.Exception)}" : $"{failure.FailureType}"
                : "NoFailure";
        }

        static string GetFullExceptionMessage(Exception exception)
        {
            if (exception.InnerException != null)
            {
                return $"{exception.Message}, InnerException: {exception.InnerException.Message}";
            }
            return exception.Message;
        }

        /// <inheritdoc />
        public override string ToString() => FormatMessage(this);
    }
}
