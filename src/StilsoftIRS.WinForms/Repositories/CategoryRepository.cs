using System;
using System.Collections.Generic;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal sealed class CategoryRepository : BaseRepository, ICategoryRepository
    {
        public IList<IncidentCategory> GetAll()
        {
            var result = new List<IncidentCategory>();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, Name, Description FROM {Table("IncidentCategories")} ORDER BY Name;";

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

        public IncidentCategory GetById(int id)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, Name, Description FROM {Table("IncidentCategories")} WHERE Id = @Id;";
                AddParameter(command, "@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public int Add(IncidentCategory category)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"INSERT INTO {Table("IncidentCategories")} (Name, Description) VALUES (@Name, @Description); " +
                    DbConnectionFactory.IdentitySelectStatement;
                AddParameter(command, "@Name", category.Name);
                AddParameter(command, "@Description", category.Description);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(IncidentCategory category)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"UPDATE {Table("IncidentCategories")} SET Name = @Name, Description = @Description WHERE Id = @Id;";
                AddParameter(command, "@Name", category.Name);
                AddParameter(command, "@Description", category.Description);
                AddParameter(command, "@Id", category.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText = $"DELETE FROM {Table("IncidentCategories")} WHERE Id = @Id;";
                AddParameter(command, "@Id", id);
                command.ExecuteNonQuery();
            }
        }

        private static IncidentCategory Map(System.Data.IDataRecord record)
        {
            return new IncidentCategory
            {
                Id = Convert.ToInt32(record["Id"]),
                Name = Convert.ToString(record["Name"]),
                Description = ReadNullableString(record["Description"])
            };
        }
    }
}
