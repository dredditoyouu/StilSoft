using Microsoft.VisualStudio.TestTools.UnitTesting;
using StilsoftIRS.Utilities;

namespace StilsoftIRS.Tests
{
    [TestClass]
    public class Sha256HasherTests
    {
        [TestMethod]
        public void ComputeHash_UsesUtf8LowercaseHex()
        {
            var hash = Sha256Hasher.ComputeHash("hello");

            Assert.AreEqual("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", hash);
        }
    }
}
