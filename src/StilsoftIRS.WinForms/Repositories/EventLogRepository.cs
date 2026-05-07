using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal sealed class EventLogRepository : BaseRepository, IEventLogRepository
    {
        public int Add(EventLogEntry entry, DbConnection connection = null, DbTransaction transaction = null)
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
                        $"INSERT INTO {Table("EventLog")} (IncidentId, UserId, Action, Comment) " +
                        $"VALUES (@IncidentId, @UserId, @Action, @Comment); " +
                        DbConnectionFactory.IdentitySelectStatement;

                    AddParameter(command, "@IncidentId", entry.IncidentId);
                    AddParameter(command, "@UserId", entry.UserId);
                    AddParameter(command, "@Action", entry.Action);
                    AddParameter(command, "@Comment", entry.Comment);
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

        public IList<EventLogEntry> GetAll(DateTime? from = null, DateTime? to = null, int? incidentId = null)
        {
            var result = new List<EventLogEntry>();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);

                var sql = new StringBuilder();
                sql.Append(
                    $"SELECT e.Id, e.IncidentId, e.UserId, e.Action, e.Comment, e.OccurredAt, " +
                    "(u.LastName + N' ' + u.FirstName) AS UserName, i.Title AS IncidentTitle " +
                    $"FROM {Table("EventLog")} e " +
                    $"INNER JOIN {Table("Users")} u ON u.Id = e.UserId " +
                    $"LEFT JOIN {Table("Incidents")} i ON i.Id = e.IncidentId " +
                    "WHERE 1 = 1 ");

                if (from.HasValue)
                {
                    sql.Append("AND e.OccurredAt >= @From ");
                    AddParameter(command, "@From", from.Value);
                }

                if (to.HasValue)
                {
                    sql.Append("AND e.OccurredAt <= @To ");
                    AddParameter(command, "@To", to.Value);
                }

                if (incidentId.HasValue)
                {
                    sql.Append("AND e.IncidentId = @IncidentId ");
                    AddParameter(command, "@IncidentId", incidentId.Value);
                }

                sql.Append("ORDER BY e.OccurredAt DESC, e.Id DESC;");
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

        private static EventLogEntry Map(System.Data.IDataRecord record)
        {
            return new EventLogEntry
            {
                Id = Convert.ToInt32(record["Id"]),
                IncidentId = ReadNullableInt(record["IncidentId"]),
                UserId = Convert.ToInt32(record["UserId"]),
                Action = Convert.ToString(record["Action"]),
                Comment = ReadNullableString(record["Comment"]),
                OccurredAt = Convert.ToDateTime(record["OccurredAt"]),
                UserName = Convert.ToString(record["UserName"]),
                IncidentTitle = ReadNullableString(record["IncidentTitle"])
            };
        }
    }
}
