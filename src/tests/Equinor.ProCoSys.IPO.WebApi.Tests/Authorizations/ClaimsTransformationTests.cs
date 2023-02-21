﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Authorizations
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
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IPlantCache> _plantCacheMock;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock.SetupGet(p => p.Plant).Returns(Plant1);

            _plantCacheMock = new Mock<IPlantCache>();
            _plantCacheMock.Setup(p => p.HasUserAccessToPlantAsync(Plant1, Oid)).Returns(Task.FromResult(true));
            _plantCacheMock.Setup(p => p.HasUserAccessToPlantAsync(Plant2, Oid)).Returns(Task.FromResult(true));

            var permissionCacheMock = new Mock<IPermissionCache>();
            permissionCacheMock.Setup(p => p.GetPermissionsForUserAsync(Plant1, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Permission1_Plant1, Permission2_Plant1}));
            permissionCacheMock.Setup(p => p.GetProjectsForUserAsync(Plant1, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Project1_Plant1, Project2_Plant1}));

            permissionCacheMock.Setup(p => p.GetPermissionsForUserAsync(Plant2, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Permission1_Plant2}));
            permissionCacheMock.Setup(p => p.GetProjectsForUserAsync(Plant2, Oid))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Project1_Plant2}));

            var loggerMock = new Mock<ILogger<ClaimsTransformation>>();

            _principalWithOid = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimsExtensions.Oid, Oid.ToString()));
            _principalWithOid.AddIdentity(claimsIdentity);
            
            _dut = new ClaimsTransformation(
                _plantProviderMock.Object,
                _plantCacheMock.Object,
                permissionCacheMock.Object,
                loggerMock.Object);
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
        public async Task TransformAsync_Twice_ShouldNotDuplicateUserDataClaimsForProjects()
        {
            await _dut.TransformAsync(_principalWithOid);
            var result = await _dut.TransformAsync(_principalWithOid);

            AssertProjectClaimsForPlant1(result.Claims);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_ForPrincipalWithoutOid()
        {
            var result = await _dut.TransformAsync(new ClaimsPrincipal());

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
        }

        [TestMethod]
        public async Task TransformAsync_ShouldNotAddAnyClaims_WhenNoPlantGiven()
        {
            _plantProviderMock.SetupGet(p => p.Plant).Returns((string)null);

            var result = await _dut.TransformAsync(_principalWithOid);

            Assert.AreEqual(0, GetProjectClaims(result.Claims).Count);
            Assert.AreEqual(0, GetRoleClaims(result.Claims).Count);
        }
        
        [TestMethod]
        public async Task TransformAsync_OnSecondPlant_ShouldClearAllClaimsForFirstPlant()
        {
            var result = await _dut.TransformAsync(_principalWithOid);
            AssertRoleClaimsForPlant1(result.Claims);
            AssertProjectClaimsForPlant1(result.Claims);

            _plantProviderMock.SetupGet(p => p.Plant).Returns(Plant2);
            result = await _dut.TransformAsync(_principalWithOid);

            var claims = GetRoleClaims(result.Claims);
            Assert.AreEqual(1, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r => r.Value == Permission1_Plant2));

            claims = GetProjectClaims(result.Claims);
            Assert.AreEqual(1, claims.Count);
            Assert.IsNotNull(claims.SingleOrDefault(r => r.Value == ClaimsTransformation.GetProjectClaimValue(Project1_Plant2)));
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

        private static List<Claim> GetRoleClaims(IEnumerable<Claim> claims)
            => claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        private static List<Claim> GetProjectClaims(IEnumerable<Claim> claims)
            => claims.Where(
                    c => c.Type == ClaimTypes.UserData && c.Value.StartsWith(ClaimsTransformation.ProjectPrefix))
                .ToList();
    }
}
