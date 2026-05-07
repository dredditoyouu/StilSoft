using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using Microsoft.Data.SqlClient;

namespace StilsoftIRS.Infrastructure
{
    internal static class DbConnectionFactory
    {
        private const string ConnectionStringEnvironmentVariable = "STILSOFT_IRS_CONNECTION_STRING";
        private const string ProviderNameEnvironmentVariable = "STILSOFT_IRS_PROVIDER_NAME";
        private const string EnvironmentFileName = "StilsoftIRS.env";
        private const string DefaultProviderName = "Microsoft.Data.SqlClient";

        private static readonly Lazy<IDictionary<string, string>> EnvironmentFileValues =
            new Lazy<IDictionary<string, string>>(LoadEnvironmentFileValues);

        public static string ConnectionString
        {
            get
            {
                var environmentValue = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(environmentValue))
                {
                    return environmentValue;
                }

                var fileValue = GetEnvironmentFileValue(ConnectionStringEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(fileValue))
                {
                    return fileValue;
                }

                var connectionString = ConfigurationManager.ConnectionStrings["StilsoftIRS"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ConfigurationErrorsException("Строка подключения StilsoftIRS не найдена.");
                }

                return connectionString;
            }
        }

        public static string ProviderName
        {
            get
            {
                var environmentValue = Environment.GetEnvironmentVariable(ProviderNameEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(environmentValue))
                {
                    return environmentValue;
                }

                var fileValue = GetEnvironmentFileValue(ProviderNameEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(fileValue))
                {
                    return fileValue;
                }

                var providerName = ConfigurationManager.ConnectionStrings["StilsoftIRS"]?.ProviderName;
                return string.IsNullOrWhiteSpace(providerName) ? DefaultProviderName : providerName;
            }
        }

        public static string IdentitySelectStatement => "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public static string CurrentTimestampExpression => "GETDATE()";

        public static DbProviderFactory ProviderFactory => SqlClientFactory.Instance;

        public static DbConnection CreateConnection()
        {
            var connection = ProviderFactory.CreateConnection();
            if (connection == null)
            {
                throw new ConfigurationErrorsException("Не удалось создать подключение к базе данных.");
            }

            connection.ConnectionString = ConnectionString;
            return connection;
        }

        public static string QualifyTable(string tableName)
        {
            return "dbo." + tableName;
        }

        private static string GetEnvironmentFileValue(string key)
        {
            return EnvironmentFileValues.Value.TryGetValue(key, out var value) ? value : null;
        }

        private static IDictionary<string, string> LoadEnvironmentFileValues()
        {
            foreach (var path in GetEnvironmentFileCandidates())
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                return ParseEnvironmentFile(path);
            }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> GetEnvironmentFileCandidates()
        {
            var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (baseDirectory != null)
            {
                yield return Path.Combine(baseDirectory.FullName, EnvironmentFileName);
                baseDirectory = baseDirectory.Parent;
            }
        }

        private static IDictionary<string, string> ParseEnvironmentFile(string path)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = line.Substring(0, separatorIndex).Trim();
                var value = line.Substring(separatorIndex + 1).Trim();

                if (value.Length >= 2)
                {
                    var doubleQuoted = value.StartsWith("\"", StringComparison.Ordinal) &&
                                       value.EndsWith("\"", StringComparison.Ordinal);
                    var singleQuoted = value.StartsWith("'", StringComparison.Ordinal) &&
                                       value.EndsWith("'", StringComparison.Ordinal);

                    if (doubleQuoted || singleQuoted)
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                }

                values[key] = value;
            }

            return values;
        }
    }
}
