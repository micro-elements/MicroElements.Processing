using System;
using System.Runtime.Serialization;

namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Исключение, которое генерируется при возникновении ошибки, если установлен <see cref="FailureHandle.Throw"/> 
    /// </summary>
    [Serializable]
    public class FailureException : Exception
    {
        public FailureException() { }

        protected FailureException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public FailureException(string message) : base(message) { }

        public FailureException(string message, Exception innerException) : base(message, innerException) { }
    }
}
