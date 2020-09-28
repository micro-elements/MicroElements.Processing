namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Контекст успешного выполнения.
    /// </summary>
    /// <typeparam name="TArg1">Тип ключа.</typeparam>
    /// <typeparam name="TResult">Тип значения.</typeparam>
    public class SuccessContext<TResult, TArg1>
    {
        public TArg1 Arg1 { get; }
        public TResult Result { get; }

        /// <inheritdoc />
        public SuccessContext(TArg1 arg1, TResult result)
        {
            Arg1 = arg1;
            Result = result;
        }
    }

    public class SuccessContext<TResult, TArg1, TArg2>
    {
        public TArg1 Arg1 { get; }
        public TArg2 Arg2 { get; }
        public TResult Result { get; }

        /// <inheritdoc />
        public SuccessContext(TArg1 arg1, TArg2 arg2, TResult result)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Result = result;
        }
    }
}
