using System.Collections.Generic;
using System.Security.Claims;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Authorizations
{
    [TestClass]
    public class ClaimsExtensionTests
    {
        [TestMethod]
        public void TryGetOid_ShouldReturnGuid_WhenOidClaimExists()
        {
            // Arrange
            var oid = "50e2322b-1990-42f4-86ac-179c7c075574";
            var claims = new List<Claim> {new Claim(ClaimsExtensions.Oid, oid)};
            
            // Act
            var guid = claims.TryGetOid();

            // Assert
            Assert.IsTrue(guid.HasValue);
            Assert.AreEqual(oid.ToLower(), guid.Value.ToString().ToLower());
        }

        [TestMethod]
        public void TryGetOid_ShouldReturnNull_WhenOidClaimNotExists()
        {
            // Arrange
            var oid = "50e2322b-1990-42f4-86ac-179c7c075574";
            var claims = new List<Claim> {new Claim(ClaimTypes.UserData, oid)};
            
            // Act
            var guid = claims.TryGetOid();

            // Assert
            Assert.IsFalse(guid.HasValue);
        }

        [TestMethod]
        public void TryGetGivenName_ShouldReturnGivenName_WhenGivenNameClaimExists()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.GivenName, "Anne") };

            var givenName = claims.TryGetGivenName();

            Assert.IsNotNull(givenName);
            Assert.AreEqual("Anne", givenName);
        }

        [TestMethod]
        public void TryGetGivenName_ShouldReturnGivenName_WhenNameClaimExists()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "Anne Knutsdotter") };

            var givenName = claims.TryGetGivenName();

            Assert.IsNotNull(givenName);
            Assert.AreEqual("Anne", givenName);
        }

        [TestMethod]
        public void TryGetGivenName_ShouldReturnNull_WhenNameClaimIsEmpty()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "") };

            var givenName = claims.TryGetGivenName();

            Assert.IsNull(givenName);
        }

        [TestMethod]
        public void TryGetGivenName_ShouldReturnNull_WhenNameClaimIsWhitespace()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "  ") };

            var givenName = claims.TryGetGivenName();

            Assert.IsNull(givenName);
        }

        [TestMethod]
        public void TryGetGivenName_ShouldReturnNull_WhenNameClaimIsOneName()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "Anne") };

            var givenName = claims.TryGetGivenName();

            Assert.IsNull(givenName);
        }

        [TestMethod]
        public void TryGetGivenName_ShouldReturnNull_WhenGivenNameDoesNotExist()
        {
            var claims = new List<Claim> { };

            var givenName = claims.TryGetGivenName();

            Assert.IsNull(givenName);
        }

        [TestMethod]
        public void TryGetSurName_ShouldReturnSurname_WhenSurNameClaimExists()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Surname, "Knutsdotter") };

            var surname = claims.TryGetSurName();

            Assert.AreEqual("Knutsdotter", surname);
        }

        [TestMethod]
        public void TryGetSurName_ShouldReturnSurname_WhenNameClaimExists()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "Anne Knutsdotter") };

            var surname = claims.TryGetSurName();

            Assert.AreEqual("Knutsdotter", surname);
        }

        [TestMethod]
        public void TryGetSurName_ShouldReturnNull_WhenNameClaimIsEmpty()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "") };

            var surname = claims.TryGetSurName();

            Assert.IsNull(surname);
        }

        [TestMethod]
        public void TryGetSurName_ShouldReturnNull_WhenNameClaimIsWhitespace()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "  ") };

            var surname = claims.TryGetSurName();

            Assert.IsNull(surname);
        }

        [TestMethod]
        public void TryGetSurName_ShouldReturnNull_WhenNameClaimIsOneName()
        {
            var claims = new List<Claim> { new Claim(ClaimsExtensions.Name, "Anne") };

            var surname = claims.TryGetSurName();

            Assert.IsNull(surname);
        }

        [TestMethod]
        public void TryGetSurName_ShouldReturnNull_WhenGivenNameDoesNotExist()
        {
            var claims = new List<Claim> { };

            var surname = claims.TryGetSurName();

            Assert.IsNull(surname);
        }
    }
}
