using System;
using System.Collections.Generic;
using System.Data.Common;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal sealed class ResourceRepository : BaseRepository, IResourceRepository
    {
        public IList<ResponseResource> GetAll()
        {
            var result = new List<ResponseResource>();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, Name, ResourceType, Responsible, IsAvailable FROM {Table("ResponseResources")} ORDER BY Name;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Map(reader));
                    }
                }
            }

            return result;
        }

        public IList<ResponseResource> GetAvailable()
        {
            var result = new List<ResponseResource>();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, Name, ResourceType, Responsible, IsAvailable " +
                    $"FROM {Table("ResponseResources")} WHERE IsAvailable = 1 ORDER BY Name;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Map(reader));
                    }
                }
            }

            return result;
        }

        public ResponseResource GetById(int id, DbConnection connection = null, DbTransaction transaction = null)
        {
            var ownConnection = connection == null;

            try
            {
                if (ownConnection)
                {
                    connection = CreateConnection();
                }

                EnsureOpen(connection);

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText =
                        $"SELECT Id, Name, ResourceType, Responsible, IsAvailable " +
                        $"FROM {Table("ResponseResources")} WHERE Id = @Id;";
                    AddParameter(command, "@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        return reader.Read() ? Map(reader) : null;
                    }
                }
            }
            finally
            {
                if (ownConnection && connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        public int Add(ResponseResource resource)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"INSERT INTO {Table("ResponseResources")} (Name, ResourceType, Responsible, IsAvailable) " +
                    $"VALUES (@Name, @ResourceType, @Responsible, @IsAvailable); " +
                    DbConnectionFactory.IdentitySelectStatement;

                FillParameters(command, resource);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(ResponseResource resource)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"UPDATE {Table("ResponseResources")} " +
                    "SET Name = @Name, ResourceType = @ResourceType, Responsible = @Responsible, IsAvailable = @IsAvailable " +
                    "WHERE Id = @Id;";

                FillParameters(command, resource);
                AddParameter(command, "@Id", resource.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText = $"DELETE FROM {Table("ResponseResources")} WHERE Id = @Id;";
                AddParameter(command, "@Id", id);
                command.ExecuteNonQuery();
            }
        }

        public void SetAvailability(int resourceId, bool isAvailable, DbConnection connection = null, DbTransaction transaction = null)
        {
            var ownConnection = connection == null;

            try
            {
                if (ownConnection)
                {
                    connection = CreateConnection();
                }

                EnsureOpen(connection);

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText =
                        $"UPDATE {Table("ResponseResources")} SET IsAvailable = @IsAvailable WHERE Id = @Id;";
                    AddParameter(command, "@IsAvailable", isAvailable);
                    AddParameter(command, "@Id", resourceId);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (ownConnection && connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        private static void FillParameters(DbCommand command, ResponseResource resource)
        {
            AddParameter(command, "@Name", resource.Name);
            AddParameter(command, "@ResourceType", resource.ResourceType);
            AddParameter(command, "@Responsible", resource.Responsible);
            AddParameter(command, "@IsAvailable", resource.IsAvailable);
        }

        private static ResponseResource Map(System.Data.IDataRecord record)
        {
            return new ResponseResource
            {
                Id = Convert.ToInt32(record["Id"]),
                Name = Convert.ToString(record["Name"]),
                ResourceType = ReadNullableString(record["ResourceType"]),
                Responsible = ReadNullableString(record["Responsible"]),
                IsAvailable = Convert.ToBoolean(record["IsAvailable"])
            };
        }
    }
}
