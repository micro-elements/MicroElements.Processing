using System;
using System.Threading.Tasks;
using Polly;
using Sberbank.Pfe2.Domain.Model;

namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Шаг получения данных.
    /// </summary>
    /// <typeparam name="TValue">Тип данных.</typeparam>
    public class GetValueStepBase<TValue>
    {
        /// <summary>
        /// Имя источника данных.
        /// </summary>
        public string SourceName { get; set; } = "Undefined";

        /// <summary>
        /// Действие, которое выполняется при успешном получении данных.
        /// Например логирование успешности получения.
        /// </summary>
        public Action<TValue> OnSuccessBase { get; set; }

        /// <summary>
        /// Действие, выполняемое в случае неуспеха (исключение или нулевой результат).
        /// Если не задано, то будет переход к следущему шагу без инициирования ошибки <see cref="FailureHandle.LogAndContinue"/>.
        /// </summary>
        public Action<FailureContext> OnFailure { get; set; }

        /// <summary>
        /// Политика выполнения шага получения данных.
        /// </summary>
        public IAsyncPolicy ExecutionPolicy { get; set; }
    }

    /// <summary>
    /// Шаг получения данных.
    /// </summary>
    /// <typeparam name="TValue">Получаемый тип данных.</typeparam>
    public class GetValueStep<TValue> : GetValueStepBase<TValue>
    {
        public delegate Task<DataSource<TValue>> GetValue();

        /// <summary>
        /// Таск получения данных.
        /// </summary>
        public GetValue GetValueTask { get; set; }
    }

    /// <summary>
    /// Шаг получения данных с одним входным параметром.
    /// </summary>
    /// <typeparam name="TValue">Получаемый тип данных.</typeparam>
    /// <typeparam name="TArg1">Аргумент для получения данных.</typeparam>
    public class GetValueStep<TValue, TArg1> : GetValueStepBase<TValue>
    {
        public delegate Task<DataSource<TValue>> GetValue(TArg1 arg1);

        /// <summary>
        /// Таск получения данных.
        /// </summary>
        public GetValue GetValueTask { get; set; }
    }

    /// <summary>
    /// Шаг получения данных с двумя входными параметрами.
    /// </summary>
    /// <typeparam name="TValue">Получаемый тип данных.</typeparam>
    /// <typeparam name="TArg1">Аргумент1 для получения данных.</typeparam>
    /// <typeparam name="TArg2">Аргумент2 для получения данных.</typeparam>
    public class GetValueStep<TValue, TArg1, TArg2> : GetValueStepBase<TValue>
    {
        public delegate Task<DataSource<TValue>> GetValue(TArg1 arg1, TArg2 arg2);

        /// <summary>
        /// Таск получения данных.
        /// </summary>
        public GetValue GetValueTask { get; set; }

        public Action<SuccessContext<TValue, TArg1, TArg2>> OnSuccess { get; set; }
    }

    /// <summary>
    /// Шаг получения данных с тремя входными параметрами.
    /// </summary>
    /// <typeparam name="TValue">Получаемый тип данных.</typeparam>
    /// <typeparam name="TArg1">Аргумент1 для получения данных.</typeparam>
    /// <typeparam name="TArg2">Аргумент2 для получения данных.</typeparam>
    /// <typeparam name="TArg3">Аргумент3 для получения данных.</typeparam>
    public class GetValueStep<TValue, TArg1, TArg2, TArg3> : GetValueStepBase<TValue>
    {
        public delegate Task<DataSource<TValue>> GetValue(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        /// Таск получения данных.
        /// </summary>
        public GetValue GetValueTask { get; set; }
    }
}
