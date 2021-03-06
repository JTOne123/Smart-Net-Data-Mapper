namespace Smart.Data.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    public sealed class DynamicParameter : IDynamicParameter
    {
        private readonly Dictionary<string, ParameterInfo> parameters = new Dictionary<string, ParameterInfo>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Performance")]
        private sealed class ParameterInfo
        {
            public readonly string Name;

            public readonly object Value;

            public readonly DbType? DbType;

            public readonly int? Size;

            public readonly ParameterDirection Direction;

            public IDbDataParameter AttachedParam;

            public ParameterInfo(string name, object value, DbType? dbType, int? size, ParameterDirection direction)
            {
                Name = name;
                Value = value;
                DbType = dbType;
                Size = size;
                Direction = direction;
            }
        }

        public void Add(string name, object value, DbType? dbType = null, int? size = null, ParameterDirection direction = ParameterDirection.Input)
        {
            parameters[name] = new ParameterInfo(name, value, dbType, size, direction);
        }

        public T Get<T>(string name)
        {
            var value = parameters[name].AttachedParam.Value;
            if (value == DBNull.Value)
            {
                return default;
            }

            return (T)value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public void Build(ISqlMapperConfig config, DbCommand cmd)
        {
            foreach (var parameter in parameters.Values)
            {
                var param = cmd.CreateParameter();
                param.ParameterName = parameter.Name;

                param.Direction = parameter.Direction;

                if ((parameter.Direction == ParameterDirection.Input) ||
                    (parameter.Direction == ParameterDirection.InputOutput))
                {
                    var value = parameter.Value;
                    if (value is null)
                    {
                        param.Value = DBNull.Value;
                    }
                    else
                    {
                        var entry = config.LookupTypeHandle(value.GetType());
                        param.DbType = parameter.DbType ?? entry.DbType;

                        if (parameter.Size.HasValue)
                        {
                            param.Size = parameter.Size.Value;
                        }

                        if (entry.TypeHandler != null)
                        {
                            entry.TypeHandler.SetValue(param, value);
                        }
                        else
                        {
                            param.Value = value;
                        }
                    }
                }

                cmd.Parameters.Add(param);
                parameter.AttachedParam = param;
            }
        }
    }
}
