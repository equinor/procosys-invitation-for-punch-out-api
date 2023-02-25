﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests.Authorization
{
    [TestClass]
    public class ClaimsTransformationTests
    {
        private ClaimsTransformation _dut;
        private Guid Oid = new Guid("{0B627D64-8113-40E1-9394-60282FB6BB9F}");
        private ClaimsPrincipal _principalWithOid;
        private readonly string Plant1 = "Plant1";
        private readonly string Plant2 = "Plant2";
        private readonly string Permission1_Plant1 = "A";
        private readonly string Permission2_Plant1 = "B";
        private readonly string Permission1_Plant2 = "C";
        private readonly string Project1_Plant1 = "Pro1";
        private readonly string Project2_Plant1 = "Pro2";
        private readonly string Project1_Plant2 = "Pro3";
        private readonly string Restriction1_Plant1 = "Res1";
        private readonly string Restriction2_Plant1 = "Res2";
        private readonly string Restriction1_Plant2 = "Res3";
        private Mock<IPersonCache> _personCacheMock;
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IAuthenticatorOptions> _authenticatorOptionsMock;

        [TestInitialize]
        public void Setup()
        {
            _personCacheMock = new Mock<IPersonCache>();
            _personCacheMock.Setup(p => p.ExistsAsync(Oid)).Returns(Task.FromResult(true));

            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock.SetupGet(p => p.Plant).Returns(Plant1);

            var permissionCacheMock = new Mock<IPermissionCache>();
            permissionCacheMock.Setup(p => p.HasUserAccessToPlantAsync(Plant1, Oid)).Returns(Task.FromResult(true));
            permissionCacheMock.Setup(p => p.HasUserAccessToPlantAsync(Plant2, Oid)).Returns(Task.FromResult(true));
            permissionCacheMock.Setup(p => p.GetPermissionsForUserAsync(Plant1, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Permission1_Plant1, Permission2_Plant1}));
            permissionCacheMock.Setup(p => p.GetProjectsForUserAsync(Plant1, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Project1_Plant1, Project2_Plant1}));
            permissionCacheMock.Setup(p => p.GetContentRestrictionsForUserAsync(Plant1, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Restriction1_Plant1, Restriction2_Plant1}));

            permissionCacheMock.Setup(p => p.GetPermissionsForUserAsync(Plant2, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Permission1_Plant2}));
            permissionCacheMock.Setup(p => p.GetProjectsForUserAsync(Plant2, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Project1_Plant2}));
            permissionCacheMock.Setup(p => p.GetContentRestrictionsForUserAsync(Plant2, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Restriction1_Plant2}));

            var loggerMock = new Mock<ILogger<ClaimsTransformation>>();

            _principalWithOid = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimsExtensions.Oid, Oid.ToString()));
            _principalWithOid.AddIdentity(claimsIdentity);

            _authenticatorOptionsMock = new Mock<IAuthenticatorOptions>();

            _dut = new ClaimsTransformation(
                _personCacheMock.Object,
                _plantProviderMock.Object,
                permissionCacheMock.Object,
                loggerMock.Object,
                _authenticatorOptionsMock.Object);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddRoleClaimsForPermissions()
        {
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRoleClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_Twice_ShouldNotDuplicateRoleClaimsForPermissions()
        {
            await _dut.TransformAsync(_principalWithOid);
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertRoleClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddUserDataClaimsForProjects()
        {
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertProjectClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddUserDataClaimsForProjects_WhenDisabled()
        {
            // Arrange
            _authenticatorOptionsMock.Setup(a => a.DisableProjectUserDataClaims).Returns(true);

            // Act
            var result = await _dut.TransformAsync(_principalWithOid);

            // Assert
            var projectClaims = GetProjectClaims(result.Claims);
            Assert.AreEqual(0, projectClaims.Count);
        }

        [TestMethod]
        public async Task TransformAsync_Twice_ShouldNotDuplicateUserDataClaimsForProjects()
        {
            await _dut.TransformAsync(_principalWithOid);
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertProjectClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldAddUserDataClaimsForContentRestriction()
        {
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertContentRestrictionForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddUserDataClaimsForContentRestriction_WhenDisabled()
        {
            // Arrange
            _authenticatorOptionsMock.Setup(a => a.DisableRestrictionRoleUserDataClaims).Returns(true);

            // Act
            var result = await _dut.TransformAsync(_principalWithOid);

            var contentRestrictionClaims = GetContentRestrictionClaims(result.Claims);
            Assert.AreEqual(0, contentRestrictionClaims.Count);
        }

        [TestMethod]
        public async Task TransformAsync_Twice_ShouldNotDuplicateUserDataClaimsForContentRestriction()
        {
            await _dut.TransformAsync(_principalWithOid);
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertContentRestrictionForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_ForPrincipalWithoutOid()
        {
            var result = await _dut.TransformAsync(new ClaimsPrincipal());

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
            Assert.AreEqual(0, GetContentRestrictionClaims(result.Claims).Count);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_WhenPersonNotFoundInProCoSys()
        {
            _personCacheMock.Setup(p => p.ExistsAsync(Oid)).Returns(Task.FromResult(false));

            var result = await _dut.TransformAsync(new ClaimsPrincipal());

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
            Assert.AreEqual(0, GetContentRestrictionClaims(result.Claims).Count);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_WhenNoPlantGiven()
        {
            _plantProviderMock.SetupGet(p => p.Plant).Returns((string)null);

            var result = await _dut.TransformAsync(_principalWithOid);

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
            Assert.AreEqual(0, GetContentRestrictionClaims(result.Claims).Count);
        }
        
        [TestMethod]
        public async Task TransformAsync_OnSecondPlant_ShouldClearAllClaimsForFirstPlant()
        {
            var result = await _dut.TransformAsync(_principalWithOid);
            AssertRoleClaimsForPlant1(result.Claims);
            AssertProjectClaimsForPlant1(result.Claims);
            AssertContentRestrictionForPlant1(result.Claims);

            _plantProviderMock.SetupGet(p => p.Plant).Returns(Plant2);
            result = await _dut.TransformAsync(_principalWithOid);

            var claims = GetRoleClaims(result.Claims);
            Assert.AreEqual(1, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r => r.Value == Permission1_Plant2));

            claims = GetProjectClaims(result.Claims);
            Assert.AreEqual(1, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r => r.Value == ClaimsTransformation.GetProjectClaimValue(Project1_Plant2)));

            claims = GetContentRestrictionClaims(result.Claims);
            Assert.AreEqual(1, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r => r.Value == ClaimsTransformation.GetContentRestrictionClaimValue(Restriction1_Plant2)));
        }

        private void AssertRoleClaimsForPlant1(IEnumerable<Claim> claims)
        {
            var roleClaims = GetRoleClaims(claims);
            Assert.AreEqual(2, roleClaims.Count);
            Assert.IsTrue(roleClaims.Any(r => r.Value == Permission1_Plant1));
            Assert.IsTrue(roleClaims.Any(r => r.Value == Permission2_Plant1));
        }

        private void AssertProjectClaimsForPlant1(IEnumerable<Claim> claims)
        {
            var projectClaims = GetProjectClaims(claims);
            Assert.AreEqual(2, projectClaims.Count);
            Assert.IsTrue(projectClaims.Any(r => r.Value == ClaimsTransformation.GetProjectClaimValue(Project1_Plant1)));
            Assert.IsTrue(projectClaims.Any(r => r.Value == ClaimsTransformation.GetProjectClaimValue(Project2_Plant1)));
        }

        private void AssertContentRestrictionForPlant1(IEnumerable<Claim> claims)
        {
            var contentRestrictionClaims = GetContentRestrictionClaims(claims);
            Assert.AreEqual(2, contentRestrictionClaims.Count);
            Assert.IsTrue(contentRestrictionClaims.Any(r => r.Value == ClaimsTransformation.GetContentRestrictionClaimValue(Restriction1_Plant1)));
            Assert.IsTrue(contentRestrictionClaims.Any(r => r.Value == ClaimsTransformation.GetContentRestrictionClaimValue(Restriction2_Plant1)));
        }

        private static List<Claim> GetContentRestrictionClaims(IEnumerable<Claim> claims)
            => claims
                .Where(c => c.Type == ClaimTypes.UserData &&
                            c.Value.StartsWith(ClaimsTransformation.ContentRestrictionPrefix))
                .ToList();

        private static List<Claim> GetRoleClaims(IEnumerable<Claim> claims)
            => claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        private static List<Claim> GetProjectClaims(IEnumerable<Claim> claims)
            => claims.Where(
                    c => c.Type == ClaimTypes.UserData && c.Value.StartsWith(ClaimsTransformation.ProjectPrefix))
                .ToList();
    }
}
