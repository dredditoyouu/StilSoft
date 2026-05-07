using System;
using System.Data.Common;

namespace StilsoftIRS.Infrastructure
{
    internal static class DbCommandHelper
    {
        public static DbParameter AddParameter(DbCommand command, string name, object value)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
            return parameter;
        }
    }
}
