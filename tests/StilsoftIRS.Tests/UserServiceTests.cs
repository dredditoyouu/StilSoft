using Microsoft.VisualStudio.TestTools.UnitTesting;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;

namespace StilsoftIRS.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        [TestMethod]
        public void Authenticate_ReturnsUser_ForValidCredentials()
        {
            var users = new FakeUserRepository();
            users.Users.Add(new User
            {
                Id = 1,
                Login = "operator",
                PasswordHash = UserService.HashPassword("Oper123"),
                Role = SystemConstants.OperatorRole,
                IsActive = true
            });

            var service = new UserService(users);
            var user = service.Authenticate("operator", "Oper123");

            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
        }

        [TestMethod]
        public void Authenticate_AllowsLegacyAdminSeedHash()
        {
            var users = new FakeUserRepository();
            users.Users.Add(new User
            {
                Id = 1,
                Login = "admin",
                PasswordHash = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
                Role = SystemConstants.AdministratorRole,
                IsActive = true
            });

            var service = new UserService(users);
            var user = service.Authenticate("admin", "Admin123");

            Assert.IsNotNull(user);
            Assert.AreEqual("admin", user.Login);
        }
    }
}
