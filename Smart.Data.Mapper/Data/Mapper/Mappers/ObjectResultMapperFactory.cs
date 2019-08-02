namespace Smart.Data.Mapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using Smart.Data.Mapper.Attributes;

    public sealed class ObjectResultMapperFactory : IResultMapperFactory
    {
        public static ObjectResultMapperFactory Instance { get; } = new ObjectResultMapperFactory();

        private ObjectResultMapperFactory()
        {
        }

        public bool IsMatch(Type type) => true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public Func<IDataRecord, T> CreateMapper<T>(ISqlMapperConfig config, Type type, ColumnInfo[] columns)
        {
            var objectFactory = config.CreateFactory<T>();
            var entries = CreateMapEntries(config, type, columns);

            return record =>
            {
                var obj = objectFactory();

                for (var i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];
                    entry.Setter(obj, record.GetValue(entry.Index));
                }

                return obj;
            };
        }

        private static MapEntry[] CreateMapEntries(ISqlMapperConfig config, Type type, ColumnInfo[] columns)
        {
            var selector = config.GetPropertySelector();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsTargetProperty)
                .ToArray();

            var list = new List<MapEntry>();
            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                var pi = selector(properties, column.Name);
                if (pi == null)
                {
                    continue;
                }
                var setter = config.CreateSetter(pi);
                var defaultValue = pi.PropertyType.GetDefaultValue();

                var parser = config.CreateParser(column.Type, pi.PropertyType);
                if (parser == null)
                {
                    list.Add(new MapEntry(i, CreateParser(setter, defaultValue)));
                }
                else
                {
                    list.Add(new MapEntry(i, CreateParser(setter, defaultValue, parser)));
                }
            }

            return list.ToArray();
        }

        private static Action<object, object> CreateParser(Action<object, object> setter, object defaultValue)
        {
            return (obj, value) => setter(obj, value is DBNull ? defaultValue : value);
        }

        private static Action<object, object> CreateParser(Action<object, object> setter, object defaultValue, Func<object, object> parser)
        {
            return (obj, value) => setter(obj, value is DBNull ? defaultValue : parser(value));
        }

        private static bool IsTargetProperty(PropertyInfo pi)
        {
            return pi.CanWrite && (pi.GetCustomAttribute<IgnoreAttribute>() == null);
        }

        private sealed class MapEntry
        {
            public int Index { get; }

            public Action<object, object> Setter { get; }

            public MapEntry(int index, Action<object, object> setter)
            {
                Index = index;
                Setter = setter;
            }
        }
    }
}
