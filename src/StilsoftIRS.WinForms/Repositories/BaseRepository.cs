using System;
using System.Data;
using System.Data.Common;
using StilsoftIRS.Infrastructure;

namespace StilsoftIRS.Repositories
{
    internal abstract class BaseRepository
    {
        protected DbConnection CreateConnection()
        {
            return DbConnectionFactory.CreateConnection();
        }

        protected static string Table(string name)
        {
            return DbConnectionFactory.QualifyTable(name);
        }

        protected static DbParameter AddParameter(DbCommand command, string name, object value)
        {
            return DbCommandHelper.AddParameter(command, name, value);
        }

        protected static string ReadNullableString(object value)
        {
            return value == DBNull.Value ? null : Convert.ToString(value);
        }

        protected static int? ReadNullableInt(object value)
        {
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        protected static DateTime? ReadNullableDateTime(object value)
        {
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        protected static void EnsureOpen(DbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }
    }
}
