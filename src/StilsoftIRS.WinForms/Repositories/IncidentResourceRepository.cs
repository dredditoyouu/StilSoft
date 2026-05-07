using System;
using System.Collections.Generic;
using System.Data.Common;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal sealed class IncidentResourceRepository : BaseRepository, IIncidentResourceRepository
    {
        public IList<IncidentResource> GetByIncidentId(int incidentId)
        {
            var result = new List<IncidentResource>();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT ir.Id, ir.IncidentId, ir.ResourceId, ir.AssignedAt, r.Name AS ResourceName, r.ResourceType, r.Responsible " +
                    $"FROM {Table("IncidentResources")} ir " +
                    $"INNER JOIN {Table("ResponseResources")} r ON r.Id = ir.ResourceId " +
                    "WHERE ir.IncidentId = @IncidentId " +
                    "ORDER BY ir.AssignedAt DESC;";
                AddParameter(command, "@IncidentId", incidentId);

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

        public bool Exists(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null)
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
                        $"SELECT COUNT(1) FROM {Table("IncidentResources")} WHERE IncidentId = @IncidentId AND ResourceId = @ResourceId;";
                    AddParameter(command, "@IncidentId", incidentId);
                    AddParameter(command, "@ResourceId", resourceId);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
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

        public int Add(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null)
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
                        $"INSERT INTO {Table("IncidentResources")} (IncidentId, ResourceId) VALUES (@IncidentId, @ResourceId); " +
                        DbConnectionFactory.IdentitySelectStatement;
                    AddParameter(command, "@IncidentId", incidentId);
                    AddParameter(command, "@ResourceId", resourceId);
                    return Convert.ToInt32(command.ExecuteScalar());
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

        public void Delete(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null)
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
                        $"DELETE FROM {Table("IncidentResources")} WHERE IncidentId = @IncidentId AND ResourceId = @ResourceId;";
                    AddParameter(command, "@IncidentId", incidentId);
                    AddParameter(command, "@ResourceId", resourceId);
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

        private static IncidentResource Map(System.Data.IDataRecord record)
        {
            return new IncidentResource
            {
                Id = Convert.ToInt32(record["Id"]),
                IncidentId = Convert.ToInt32(record["IncidentId"]),
                ResourceId = Convert.ToInt32(record["ResourceId"]),
                AssignedAt = Convert.ToDateTime(record["AssignedAt"]),
                ResourceName = Convert.ToString(record["ResourceName"]),
                ResourceType = ReadNullableString(record["ResourceType"]),
                Responsible = ReadNullableString(record["Responsible"])
            };
        }
    }
}
