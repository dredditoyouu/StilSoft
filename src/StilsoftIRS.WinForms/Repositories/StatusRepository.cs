using System;
using System.Collections.Generic;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal sealed class StatusRepository : BaseRepository, IStatusRepository
    {
        public IList<IncidentStatus> GetAll()
        {
            var result = new List<IncidentStatus>();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, Name, ColorHex FROM {Table("IncidentStatuses")} " +
                    "ORDER BY CASE Name " +
                    "WHEN N'Новый' THEN 1 " +
                    "WHEN N'В работе' THEN 2 " +
                    "WHEN N'Эскалирован' THEN 3 " +
                    "WHEN N'Решён' THEN 4 " +
                    "WHEN N'Закрыт' THEN 5 " +
                    "ELSE 99 END;";

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

        public IncidentStatus GetById(int id)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText = $"SELECT Id, Name, ColorHex FROM {Table("IncidentStatuses")} WHERE Id = @Id;";
                AddParameter(command, "@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public IncidentStatus GetByName(string name)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText = $"SELECT Id, Name, ColorHex FROM {Table("IncidentStatuses")} WHERE Name = @Name;";
                AddParameter(command, "@Name", name);

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        private static IncidentStatus Map(System.Data.IDataRecord record)
        {
            return new IncidentStatus
            {
                Id = Convert.ToInt32(record["Id"]),
                Name = Convert.ToString(record["Name"]),
                ColorHex = Convert.ToString(record["ColorHex"])
            };
        }
    }
}
