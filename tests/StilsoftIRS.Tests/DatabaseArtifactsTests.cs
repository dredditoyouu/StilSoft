using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StilsoftIRS.Tests
{
    [TestClass]
    public class DatabaseArtifactsTests
    {
        [TestMethod]
        public void DatabaseScripts_ArePresentAndContainSeedAdmin()
        {
            var schemaPath = FindFile(@"src\StilsoftIRS.WinForms\Database\001_create_schema.sql");
            var seedPath = FindFile(@"src\StilsoftIRS.WinForms\Database\002_seed_data.sql");

            Assert.IsTrue(File.Exists(schemaPath));
            Assert.IsTrue(File.Exists(seedPath));
            StringAssert.Contains(File.ReadAllText(schemaPath), "CREATE TABLE dbo.Users");
            StringAssert.Contains(File.ReadAllText(seedPath), "Login = N'admin'");
        }

        private static string FindFile(string relativePath)
        {
            var currentDirectory = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);
            while (currentDirectory != null)
            {
                var candidate = Path.Combine(currentDirectory.FullName, relativePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                currentDirectory = currentDirectory.Parent;
            }

            return relativePath;
        }
    }
}
