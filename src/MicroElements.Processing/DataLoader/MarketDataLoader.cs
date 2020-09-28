using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MicroElements.Functional;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using Polly;
using Sberbank.Pfe2.Domain.Model;
using Sberbank.Pfe2.Integration.MarketData.Loader;

namespace Sberbank.Pfe2.MarketData.Loader
{
    /// <summary>
    /// Загрузчик рыночных данных.
    /// </summary>
    public class MarketDataLoader
    {
        private readonly ILogger _logger;


        public MarketDataLoader(ILoggerFactory loggerFactory = null)
            : this((loggerFactory ?? NullLoggerFactory.Instance).CreateLogger(typeof(MarketDataLoader)))
        {
        }

        private MarketDataLoader(ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public Task<IMarketData> LoadMarketData(MarketDataRequest marketDataRequest)
        {
            _logger.LogInformation("LoadMarketData started");
            _logger.LogInformation($"AsOfDate: {marketDataRequest.AsOfDate}");
            _logger.LogInformation($"MarketDataDate: {marketDataRequest.MarketDataDate}");

            return LoadMarketDataAsync(marketDataRequest, new MarketData<MarketDataRequest>(marketDataRequest.AsOfDate, marketDataRequest));
        }

        public async Task<IMarketData> LoadMarketData(MarketDataRequest marketDataRequest, IMarketDataSet marketDataSet, Func<LocalDate, IMarketData> factory)
        {
            // NOTE: вот тут используется AsOfDate для хранения в marketDataSet, но на самом деле данные грузятся за MarketDataDate.
            IMarketData marketData = marketDataSet.GetOrCreate(marketDataRequest.AsOfDate, factory);

            return await LoadMarketDataAsync(marketDataRequest, marketData);
        }

        public async Task<IMarketData> LoadMarketDataAsync(MarketDataRequest marketDataRequest, IMarketData container)
        {
            var bulkheadAsync = Policy.BulkheadAsync(marketDataRequest.ConcurrencyLimit, int.MaxValue);
            var parallelExecution = marketDataRequest.ConcurrencyLimit > 1;
            MarketDataContext context = new MarketDataContext(marketDataRequest, container, parallelExecution, bulkheadAsync);

            List<Task> tasks = new List<Task>();

            foreach (var dataRequest in marketDataRequest.DataRequests)
            {
                var requestTask = LoadRequest(context, dataRequest);
                if (!context.ParallelExecution)
                    await requestTask;
                else
                    tasks.Add(requestTask);
            }

            if (context.ParallelExecution)
            {
                Task[] array = tasks.ToArray();
                await Task.WhenAll(array);
            }

            return container;
        }

        private Task LoadRequest(MarketDataContext context, IDataRequest dataRequest)
        {
            var loadDataMethod = typeof(MarketDataLoader).GetMethod(nameof(LoadData), BindingFlags.Instance | BindingFlags.NonPublic);
            var loadDataTyped = loadDataMethod.MakeGenericMethod(dataRequest.ResultType, dataRequest.KeyType);

            var task = (Task)loadDataTyped.Invoke(this,
                new[]
                {
                    context,
                    dataRequest.GetPropertyValue("Keys"),
                    dataRequest.GetPropertyValue("GetValueSteps"),
                    null,//Result
                    dataRequest.GetPropertyValue(nameof(DataRequest<string, string>.OnResult)),
                    dataRequest.GetPropertyValue("Name")
                });
            return task;
        }

        /// <summary>
        /// Обобщенный метод получения данных.
        /// </summary>
        private async Task LoadData<TValue, TKey>(
            MarketDataContext context,
            IEnumerable<TKey> keys,
            IEnumerable<GetValueStep<TValue, LocalDate, TKey>> getValueSteps,
            IDictionary<TKey, DataSource<TValue>> result,
            Action<ResultContext<TValue, TKey>> onResult,
            string dataName = "data")
        {
            if (keys == null)
                return;

            var steps = getValueSteps.ToList();

            if (context.MarketDataRequest.SourcesToLoad != null && context.MarketDataRequest.SourcesToLoad.Count > 0)
            {
                steps = steps.Where(step => context.MarketDataRequest.SourcesToLoad.Contains(step.SourceName)).ToList();
            }
            if (context.MarketDataRequest.SourcesToSkip != null && context.MarketDataRequest.SourcesToSkip.Count > 0)
            {
                steps = steps.Where(step => !context.MarketDataRequest.SourcesToSkip.Contains(step.SourceName)).ToList();
            }

            if (context.ParallelExecution)
            {
                var getDataTasks = keys.Select(key => context.ExecutionPolicy.ExecuteAsync(() => RunGetData(context, key, steps, result, onResult, dataName))).ToArray();
                await Task.WhenAll(getDataTasks);
            }
            else
            {
                // Итерация по ключам, которые нужно получить.
                foreach (var key in keys)
                {
                    await RunGetData(context, key, steps, result, onResult, dataName);
                }
            }
        }

        private async Task RunGetData<TValue, TKey>(
            MarketDataContext context,
            TKey key,
            IReadOnlyList<GetValueStep<TValue, LocalDate, TKey>> getValueSteps,
            IDictionary<TKey, DataSource<TValue>> result,
            Action<ResultContext<TValue, TKey>> onResult,
            string dataName)
        {
            await Task.Yield();

            var marketDataRequest = context.MarketDataRequest;
            var marketData = context.MarketData;

            FillStepsData(getValueSteps, marketDataRequest);

            // Запуск шагов получения данных
            foreach (var getValueStep in getValueSteps)
            {
                string GetErrorMessage(FailureContext failure, long elapsedMs) =>
                    $"{dataName} failed to load from {getValueStep.SourceName}. MarketDataDate:{marketDataRequest.MarketDataDate:yyyy-MM-dd}, Key:{key}, ElapsedMs: {elapsedMs}, Failure:{FailureContext.FormatMessage(failure)}";

                FailureContext failureContext = null;
                var loadingTime = Stopwatch.StartNew();
                try
                {
                    // Получение данных.
                    _logger.LogDebug($"Started getting {dataName} {key} from {getValueStep.SourceName}");

                    DataSource<TValue> data;
                    if (getValueStep.ExecutionPolicy != null)
                    {
                        // Запускаем получение данных через установленную политику запуска.
                        data = await getValueStep.ExecutionPolicy.ExecuteAsync(async () => await getValueStep.GetValueTask(marketDataRequest.MarketDataDate, key));
                    }
                    else
                    {
                        // Запускаем получение данных.
                        data = await getValueStep.GetValueTask(marketDataRequest.MarketDataDate, key);
                    }

                    if (data != null && data.Type == SourceType.BadMarketData)
                    {
                        _logger.LogWarning($"{dataName} is BadMarketData.  Key: {key}, ElapsedMs: {loadingTime.ElapsedMilliseconds}");
                        break;
                    }

                    if (data != null && data.Value != null && data.Type != SourceType.NoValue)
                    {
                        data = data.EnsureFilled(key);
                        if (result != null)
                            result[key] = data;
                        onResult?.Invoke(new ResultContext<TValue, TKey>(marketDataRequest.MarketDataDate, key, data));

                        getValueStep.OnSuccess?.Invoke(new SuccessContext<TValue, LocalDate, TKey>(marketDataRequest.MarketDataDate, key, data));
                        getValueStep.OnSuccessBase?.Invoke(data);

                        var infoMessage = $"{dataName} loaded from {getValueStep.SourceName}. Key: {key}, ElapsedMs: {loadingTime.ElapsedMilliseconds}";
                        marketData.Messages.Add(new Message(infoMessage, MessageSeverity.Information));
                        _logger.LogInformation(infoMessage);
                        break;
                    }
                }
                catch (Exception e)
                {
                    failureContext = new FailureContext(FailureType.Exception, e);
                    getValueStep.OnFailure?.Invoke(failureContext);
                }
                finally
                {
                    string optionalFailMessage = failureContext != null ? $", Failure:{FailureContext.FormatMessage(failureContext)}" : string.Empty;
                    _logger.LogDebug($"Finished getting {dataName} {key} from {getValueStep.SourceName}. ElapsedMs: {loadingTime.ElapsedMilliseconds}{optionalFailMessage}");
                }

                #region FailureAction Processing

                if (failureContext == null)
                {
                    failureContext = new FailureContext(FailureType.NullResult);
                    getValueStep.OnFailure?.Invoke(failureContext);
                }

                var errorMessageWithElapsed = GetErrorMessage(failureContext, loadingTime.ElapsedMilliseconds);

                if (failureContext.OnFailure == FailureHandle.Continue)
                {
                    _logger.LogDebug(failureContext.Exception, errorMessageWithElapsed);
                    continue;
                }

                if (failureContext.OnFailure == FailureHandle.LogAndContinue)
                {
                    marketData.Messages.Add(new Message(errorMessageWithElapsed, MessageSeverity.Information));
                    _logger.LogInformation(_logger.IsEnabled(LogLevel.Debug) ? failureContext.Exception : null, errorMessageWithElapsed);
                    continue;
                }

                if (failureContext.OnFailure == FailureHandle.LogWarnAndContinue)
                {
                    marketData.Messages.Add(new Message(errorMessageWithElapsed, MessageSeverity.Warning));
                    _logger.LogWarning(_logger.IsEnabled(LogLevel.Debug) ? failureContext.Exception : null, errorMessageWithElapsed);
                    continue;
                }

                if (failureContext.OnFailure == FailureHandle.BreakWithError)
                {
                    marketData.Messages.AddError(failureContext.Exception, errorMessageWithElapsed);
                    _logger.LogError(failureContext.Exception, errorMessageWithElapsed);
                    break;
                }

                if (failureContext.OnFailure == FailureHandle.Throw)
                {
                    marketData.Messages.Add(new Message(errorMessageWithElapsed, MessageSeverity.Error));
                    _logger.LogError(failureContext.Exception, errorMessageWithElapsed);
                    throw new FailureException(errorMessageWithElapsed);
                }

                #endregion
            }
        }

        private void FillStepsData<TValue, TKey>(IReadOnlyList<GetValueStep<TValue, LocalDate, TKey>> getValueSteps, MarketDataRequest marketDataRequest)
        {
            for (var i = 0; i < getValueSteps.Count; i++)
            {
                bool isLast = i == getValueSteps.Count - 1;
                var getValueStep = getValueSteps[i];
                if (getValueStep.OnFailure == null)
                {
                    if (isLast)
                    {
                        // Для последнего шага обычно FailureHandle.BreakWithError
                        getValueStep.OnFailure = failureContext => failureContext.OnFailure = marketDataRequest.DefaultFailureHandleForLastStep;
                    }
                    else
                    {
                        // Для промежуточных шагов обычно FailureHandle.LogAndContinue
                        getValueStep.OnFailure = failureContext => failureContext.OnFailure = marketDataRequest.DefaultFailureHandleForNotLastStep;
                    }
                }
            }
        }
    }

    public static class ReflectionExtensions
    {
        public static object GetPropertyValue(this object target, string propertyName)
        {
            return target.GetType().GetProperty(propertyName).GetValue(target);
        }
    }
}
