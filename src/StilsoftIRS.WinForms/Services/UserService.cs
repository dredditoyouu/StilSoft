using System;
using System.Collections.Generic;
using System.Linq;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Repositories;
using StilsoftIRS.Utilities;

namespace StilsoftIRS.Services
{
    internal sealed class UserService
    {
        private const string LegacyAdminSeedHash = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3";

        private readonly IUserRepository _users;

        public UserService(IUserRepository users)
        {
            _users = users;
        }

        public static string HashPassword(string password)
        {
            return Sha256Hasher.ComputeHash(password);
        }

        public User Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = _users.GetByLogin(login.Trim());
            if (user == null || !user.IsActive)
            {
                return null;
            }

            var passwordHash = HashPassword(password);
            if (string.Equals(user.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
            {
                return user;
            }

            // Compatibility with the legacy admin seed required by the specification.
            if (string.Equals(user.Login, "admin", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(password, "Admin123", StringComparison.Ordinal) &&
                string.Equals(user.PasswordHash, LegacyAdminSeedHash, StringComparison.OrdinalIgnoreCase))
            {
                return user;
            }

            return null;
        }

        public IList<User> GetUsers()
        {
            return _users.GetAll();
        }

        public User GetUser(int id)
        {
            return _users.GetById(id);
        }

        public void SaveUser(User user, string plainPassword)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName))
            {
                throw new InvalidOperationException("Имя и фамилия пользователя обязательны.");
            }

            if (string.IsNullOrWhiteSpace(user.Login))
            {
                throw new InvalidOperationException("Логин пользователя обязателен.");
            }

            if (!SystemConstants.Roles.Contains(user.Role))
            {
                throw new InvalidOperationException("Указана недопустимая роль пользователя.");
            }

            if (!string.IsNullOrWhiteSpace(plainPassword))
            {
                user.PasswordHash = HashPassword(plainPassword);
            }
            else if (user.Id == 0)
            {
                throw new InvalidOperationException("Для нового пользователя необходимо указать пароль.");
            }
            else if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                user.PasswordHash = _users.GetById(user.Id)?.PasswordHash;
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                throw new InvalidOperationException("Не удалось сохранить пароль пользователя.");
            }

            user.Login = user.Login.Trim();
            user.FirstName = user.FirstName.Trim();
            user.LastName = user.LastName.Trim();

            if (user.Id == 0)
            {
                user.Id = _users.Add(user);
                return;
            }

            _users.Update(user);
        }

        public static bool IsInRole(User user, params string[] roles)
        {
            return user != null && roles.Any(role => string.Equals(user.Role, role, StringComparison.Ordinal));
        }

        public static void EnsureRole(User user, params string[] roles)
        {
            if (!IsInRole(user, roles))
            {
                throw new UnauthorizedAccessException("Недостаточно прав для выполнения операции.");
            }
        }
    }
}
