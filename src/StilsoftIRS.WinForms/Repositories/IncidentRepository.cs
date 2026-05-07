using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal sealed class IncidentRepository : BaseRepository, IIncidentRepository
    {
        public IList<Incident> GetAll(IncidentQuery query)
        {
            var result = new List<Incident>();
            query = query ?? new IncidentQuery();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);

                var sql = new StringBuilder();
                sql.Append(
                    $"SELECT i.Id, i.Title, i.Description, i.CreatedAt, i.ClosedAt, i.Priority, i.CategoryId, i.StatusId, i.OperatorId, " +
                    "c.Name AS CategoryName, s.Name AS StatusName, s.ColorHex AS StatusColorHex, " +
                    "(u.LastName + N' ' + u.FirstName) AS OperatorName " +
                    $"FROM {Table("Incidents")} i " +
                    $"INNER JOIN {Table("IncidentCategories")} c ON c.Id = i.CategoryId " +
                    $"INNER JOIN {Table("IncidentStatuses")} s ON s.Id = i.StatusId " +
                    $"INNER JOIN {Table("Users")} u ON u.Id = i.OperatorId " +
                    "WHERE 1 = 1 ");

                if (query.StatusId.HasValue)
                {
                    sql.Append("AND i.StatusId = @StatusId ");
                    AddParameter(command, "@StatusId", query.StatusId.Value);
                }

                if (query.CategoryId.HasValue)
                {
                    sql.Append("AND i.CategoryId = @CategoryId ");
                    AddParameter(command, "@CategoryId", query.CategoryId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.Priority))
                {
                    sql.Append("AND i.Priority = @Priority ");
                    AddParameter(command, "@Priority", query.Priority);
                }

                if (!string.IsNullOrWhiteSpace(query.SearchText))
                {
                    sql.Append("AND (i.Title LIKE @Search OR i.Description LIKE @Search) ");
                    AddParameter(command, "@Search", "%" + query.SearchText.Trim() + "%");
                }

                if (query.CreatedFrom.HasValue)
                {
                    sql.Append("AND i.CreatedAt >= @CreatedFrom ");
                    AddParameter(command, "@CreatedFrom", query.CreatedFrom.Value);
                }

                if (query.CreatedTo.HasValue)
                {
                    sql.Append("AND i.CreatedAt <= @CreatedTo ");
                    AddParameter(command, "@CreatedTo", query.CreatedTo.Value);
                }

                sql.Append("ORDER BY i.CreatedAt DESC, i.Id DESC;");
                command.CommandText = sql.ToString();

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

        public IList<Incident> GetByCreatedPeriod(DateTime from, DateTime to)
        {
            return GetAll(new IncidentQuery
            {
                CreatedFrom = from,
                CreatedTo = to
            });
        }

        public Incident GetById(int id)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT i.Id, i.Title, i.Description, i.CreatedAt, i.ClosedAt, i.Priority, i.CategoryId, i.StatusId, i.OperatorId, " +
                    "c.Name AS CategoryName, s.Name AS StatusName, s.ColorHex AS StatusColorHex, " +
                    "(u.LastName + N' ' + u.FirstName) AS OperatorName " +
                    $"FROM {Table("Incidents")} i " +
                    $"INNER JOIN {Table("IncidentCategories")} c ON c.Id = i.CategoryId " +
                    $"INNER JOIN {Table("IncidentStatuses")} s ON s.Id = i.StatusId " +
                    $"INNER JOIN {Table("Users")} u ON u.Id = i.OperatorId " +
                    "WHERE i.Id = @Id;";
                AddParameter(command, "@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public int Add(Incident incident, DbConnection connection = null, DbTransaction transaction = null)
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
                        $"INSERT INTO {Table("Incidents")} (Title, Description, Priority, CategoryId, StatusId, OperatorId) " +
                        $"VALUES (@Title, @Description, @Priority, @CategoryId, @StatusId, @OperatorId); " +
                        DbConnectionFactory.IdentitySelectStatement;

                    AddParameter(command, "@Title", incident.Title);
                    AddParameter(command, "@Description", incident.Description);
                    AddParameter(command, "@Priority", incident.Priority);
                    AddParameter(command, "@CategoryId", incident.CategoryId);
                    AddParameter(command, "@StatusId", incident.StatusId);
                    AddParameter(command, "@OperatorId", incident.OperatorId);

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

        public void UpdateStatus(int incidentId, int statusId, DateTime? closedAt, DbConnection connection = null, DbTransaction transaction = null)
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
                        $"UPDATE {Table("Incidents")} SET StatusId = @StatusId, ClosedAt = @ClosedAt WHERE Id = @Id;";
                    AddParameter(command, "@StatusId", statusId);
                    AddParameter(command, "@ClosedAt", closedAt);
                    AddParameter(command, "@Id", incidentId);
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

        private static Incident Map(System.Data.IDataRecord record)
        {
            return new Incident
            {
                Id = Convert.ToInt32(record["Id"]),
                Title = Convert.ToString(record["Title"]),
                Description = ReadNullableString(record["Description"]),
                CreatedAt = Convert.ToDateTime(record["CreatedAt"]),
                ClosedAt = ReadNullableDateTime(record["ClosedAt"]),
                Priority = Convert.ToString(record["Priority"]),
                CategoryId = Convert.ToInt32(record["CategoryId"]),
                StatusId = Convert.ToInt32(record["StatusId"]),
                OperatorId = Convert.ToInt32(record["OperatorId"]),
                CategoryName = Convert.ToString(record["CategoryName"]),
                StatusName = Convert.ToString(record["StatusName"]),
                StatusColorHex = Convert.ToString(record["StatusColorHex"]),
                OperatorName = Convert.ToString(record["OperatorName"])
            };
        }
    }
}
