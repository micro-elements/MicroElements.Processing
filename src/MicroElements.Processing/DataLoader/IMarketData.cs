using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MicroElements.Functional;
using NodaTime;
using Sberbank.Pfe2.Domain.Model;

namespace Sberbank.Pfe2.MarketData
{
    /// <summary>
    /// Набор рыночных данных на определенную дату.
    /// </summary>
    public interface IMarketData
    {
        /// <summary>
        /// Дата рыночных данных.
        /// </summary>
        LocalDate Date { get; }

        /// <summary>
        /// Дополнительная метаинформация.
        /// </summary>
        ConcurrentDictionary<string, string> Properties { get; }

        /// <summary>
        /// Список сообщений.
        /// </summary>
        IMutableMessageList<Message> Messages { get; }

        IEnumerable<ResultGroup> GetResultGroups();

        /// <summary>
        /// Добавление результата в свободной форме.
        /// </summary>
        /// <typeparam name="TKey">Тип ключа ресурса.</typeparam>
        /// <typeparam name="TResult">Тип ресурса.</typeparam>
        /// <param name="sectionName">Название секции для того, чтобы можно было разводить ресурсы одного типа по группам.</param>
        /// <param name="key">Ключ ресурса.</param>
        /// <param name="result">Значение ресурса.</param>
        /// <returns>true if the key/value pair was added.</returns>
        bool TryAddResult<TKey, TResult>(string sectionName, TKey key, DataSource<TResult> result);

        /// <summary>
        /// Получение ресурса.
        /// </summary>
        /// <typeparam name="TKey">Тип ключа ресурса.</typeparam>
        /// <typeparam name="TResult">Тип ресурса.</typeparam>
        /// <param name="sectionName">Название секции для того, чтобы можно было разводить ресурсы одного типа по группам.</param>
        /// <param name="key">Ключ ресурса.</param>
        /// <returns>Ресурс или NoValue ресурс.</returns>
        DataSource<TResult> GetResult<TKey, TResult>(string sectionName, TKey key);
    }

    /// <summary>
    /// Группа результатов.
    /// </summary>
    public class ResultGroup
    {
        private static IList<Type> _assemblies;

        /// <inheritdoc />
        public ResultGroup(string section, Type keyType, Type dataType, object dictionary)
        {
            Section = section;
            KeyType = keyType;
            DataType = dataType;
            Dictionary = dictionary;
        }

        public static ResultGroup FromMetaData(Metadata metadata)
        {
            LazyInitializer.EnsureInitialized(ref _assemblies, GetAssemblies);

            var section = metadata.Section;

            var keyType = Type.GetType(metadata.KeyType)
                      ?? Type.GetType(_assemblies.First(t => t.AssemblyQualifiedName.Contains(metadata.KeyType)).AssemblyQualifiedName);

            var dataType = Type.GetType(metadata.DataType)
                       ?? Type.GetType(_assemblies.First(t => t.AssemblyQualifiedName.Contains(metadata.DataType)).AssemblyQualifiedName);

            var ds = typeof(DataSource<>).MakeGenericType(dataType);
            var cd = typeof(ConcurrentDictionary<,>).MakeGenericType(keyType, ds);

            var dictionary = Activator.CreateInstance(cd);

            return new ResultGroup(section, keyType, dataType, dictionary);

        }

        private static IList<Type> GetAssemblies()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.DefinedTypes.Select(t => t.AsType());
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                })
                .ToArray();

        }

        public string Section { get; }
        public Type KeyType { get; }
        public Type DataType { get; }
        public object Dictionary { get; }
    }

    public class Metadata
    {
        public Metadata(string section, string keyType, string dataType)
        {
            Section = section;
            KeyType = keyType;
            DataType = dataType;
        }

        public static Metadata FromResultGroup(ResultGroup resultGroup)
        {
            Type resultGroupDataType = resultGroup.DataType;
            string dataType;
            if (resultGroupDataType.IsInterface && resultGroupDataType.FullName.Contains("Sberbank"))
                dataType = resultGroupDataType.AssemblyQualifiedName;
            else
                dataType = resultGroupDataType.ToString();

            return new Metadata(resultGroup.Section, resultGroup.KeyType.ToString(), dataType);
        }

        public string Section { get; set; }
        public string KeyType { get; set; }
        public string DataType { get; set; }
    }
}
