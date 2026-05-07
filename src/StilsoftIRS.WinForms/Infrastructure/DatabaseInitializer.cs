using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace StilsoftIRS.Infrastructure
{
    internal static class DatabaseInitializer
    {
        private const string SchemaScriptEnvironmentVariable = "STILSOFT_IRS_SCHEMA_SCRIPT";
        private const string SeedScriptEnvironmentVariable = "STILSOFT_IRS_SEED_SCRIPT";

        public static void Initialize()
        {
            EnsureDatabaseExists();
            EnsureSchema();
            EnsureSeedData();
        }

        private static void EnsureDatabaseExists()
        {
            var builder = new SqlConnectionStringBuilder(DbConnectionFactory.ConnectionString);
            var databaseName = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ConfigurationErrorsException("В строке подключения не указано имя базы данных.");
            }

            builder.InitialCatalog = "master";

            using (var connection = new SqlConnection(builder.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText =
                    "IF DB_ID(@DatabaseName) IS NULL " +
                    "BEGIN " +
                    "    DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE [' + REPLACE(@DatabaseName, ']', ']]') + N']'; " +
                    "    EXEC (@sql); " +
                    "END;";
                command.Parameters.AddWithValue("@DatabaseName", databaseName);
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureSchema()
        {
            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT COUNT(1) FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo');";

                var exists = Convert.ToInt32(command.ExecuteScalar()) > 0;
                if (!exists)
                {
                    ExecuteSqlScript(connection, ResolveScriptPath("SchemaScriptPath"));
                }
            }
        }

        private static void EnsureSeedData()
        {
            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT COUNT(1) FROM {DbConnectionFactory.QualifyTable("Users")};";
                var hasUsers = Convert.ToInt32(command.ExecuteScalar()) > 0;
                if (!hasUsers)
                {
                    ExecuteSqlScript(connection, ResolveScriptPath("SeedScriptPath"));
                }
            }
        }

        private static string ResolveScriptPath(string key)
        {
            var environmentKey = key == "SchemaScriptPath"
                ? SchemaScriptEnvironmentVariable
                : SeedScriptEnvironmentVariable;

            var environmentPath = Environment.GetEnvironmentVariable(environmentKey);
            if (!string.IsNullOrWhiteSpace(environmentPath) && File.Exists(environmentPath))
            {
                return environmentPath;
            }

            var configuredRelativePath = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(configuredRelativePath))
            {
                configuredRelativePath = key == "SchemaScriptPath"
                    ? @"Database\001_create_schema.sql"
                    : @"Database\002_seed_data.sql";
            }

            foreach (var path in ExpandCandidatePaths(configuredRelativePath))
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            throw new FileNotFoundException("SQL-скрипт не найден: " + configuredRelativePath);
        }

        private static IEnumerable<string> ExpandCandidatePaths(string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
            {
                yield return relativePath;
                yield break;
            }

            var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (currentDirectory != null)
            {
                yield return Path.Combine(currentDirectory.FullName, relativePath);
                yield return Path.Combine(currentDirectory.FullName, "src", "StilsoftIRS.WinForms", relativePath);
                currentDirectory = currentDirectory.Parent;
            }
        }

        private static void ExecuteSqlScript(DbConnection connection, string scriptPath)
        {
            var script = File.ReadAllText(scriptPath);
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (var batch in batches)
            {
                var sql = batch.Trim();
                if (string.IsNullOrWhiteSpace(sql))
                {
                    continue;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
