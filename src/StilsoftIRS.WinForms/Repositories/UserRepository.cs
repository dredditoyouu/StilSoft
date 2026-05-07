using System;
using System.Collections.Generic;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal sealed class UserRepository : BaseRepository, IUserRepository
    {
        public User GetByLogin(string login)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, FirstName, LastName, Login, PasswordHash, Role, IsActive " +
                    $"FROM {Table("Users")} WHERE Login = @Login;";
                AddParameter(command, "@Login", login);

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public User GetById(int id)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, FirstName, LastName, Login, PasswordHash, Role, IsActive " +
                    $"FROM {Table("Users")} WHERE Id = @Id;";
                AddParameter(command, "@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public IList<User> GetAll()
        {
            var result = new List<User>();

            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"SELECT Id, FirstName, LastName, Login, PasswordHash, Role, IsActive " +
                    $"FROM {Table("Users")} ORDER BY LastName, FirstName, Login;";

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

        public int Add(User user)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"INSERT INTO {Table("Users")} (FirstName, LastName, Login, PasswordHash, Role, IsActive) " +
                    $"VALUES (@FirstName, @LastName, @Login, @PasswordHash, @Role, @IsActive); " +
                    DbConnectionFactory.IdentitySelectStatement;

                FillSaveParameters(command, user);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(User user)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                EnsureOpen(connection);
                command.CommandText =
                    $"UPDATE {Table("Users")} " +
                    "SET FirstName = @FirstName, LastName = @LastName, Login = @Login, PasswordHash = @PasswordHash, Role = @Role, IsActive = @IsActive " +
                    "WHERE Id = @Id;";

                FillSaveParameters(command, user);
                AddParameter(command, "@Id", user.Id);
                command.ExecuteNonQuery();
            }
        }

        private static void FillSaveParameters(System.Data.Common.DbCommand command, User user)
        {
            AddParameter(command, "@FirstName", user.FirstName);
            AddParameter(command, "@LastName", user.LastName);
            AddParameter(command, "@Login", user.Login);
            AddParameter(command, "@PasswordHash", user.PasswordHash);
            AddParameter(command, "@Role", user.Role);
            AddParameter(command, "@IsActive", user.IsActive);
        }

        private static User Map(System.Data.IDataRecord record)
        {
            return new User
            {
                Id = Convert.ToInt32(record["Id"]),
                FirstName = Convert.ToString(record["FirstName"]),
                LastName = Convert.ToString(record["LastName"]),
                Login = Convert.ToString(record["Login"]),
                PasswordHash = Convert.ToString(record["PasswordHash"]),
                Role = Convert.ToString(record["Role"]),
                IsActive = Convert.ToBoolean(record["IsActive"])
            };
        }
    }
}
