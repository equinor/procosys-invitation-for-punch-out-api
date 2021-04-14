using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeboTech.TimeService;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission;
using Equinor.ProCoSys.IPO.Infrastructure.Caching;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.WebApi.Caches;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Caches
{
    [TestClass]
    public class PermissionCacheTests
    {
        private PermissionCache _dut;
        private readonly Guid Oid = new Guid("{3BFB54C7-91E2-422E-833F-951AD07FE37F}");
        private Mock<IPermissionApiService> _permissionApiServiceMock;
        private readonly string TestPlant = "TestPlant";
        private readonly string Permission1 = "A";
        private readonly string Permission2 = "B";
        private readonly string Project1WithAccess = "P1";
        private readonly string Project2WithAccess = "P2";
        private readonly string ProjectWithoutAccess = "P3";

        [TestInitialize]
        public void Setup()
        {
            TimeService.SetConstant(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            _permissionApiServiceMock = new Mock<IPermissionApiService>();
            _permissionApiServiceMock.Setup(p => p.GetAllOpenProjectsAsync(TestPlant))
                .Returns(Task.FromResult<IList<ProCoSysProject>>(new List<ProCoSysProject>
            {
                new ProCoSysProject {Name = Project1WithAccess, HasAccess = true},
                new ProCoSysProject {Name = Project2WithAccess, HasAccess = true},
                new ProCoSysProject {Name = ProjectWithoutAccess}
            }));
            _permissionApiServiceMock.Setup(p => p.GetPermissionsAsync(TestPlant))
                .Returns(Task.FromResult<IList<string>>(new List<string> {Permission1, Permission2}));

            var optionsMock = new Mock<IOptionsMonitor<CacheOptions>>();
            optionsMock
                .Setup(x => x.CurrentValue)
                .Returns(new CacheOptions());

            _dut = new PermissionCache(new CacheManager(), _permissionApiServiceMock.Object, optionsMock.Object);
        }

        [TestMethod]
        public async Task GetPermissionsForUser_ShouldReturnPermissionsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetPermissionsForUserAsync(TestPlant, Oid);

            // Assert
            AssertPermissions(result);
            _permissionApiServiceMock.Verify(p => p.GetPermissionsAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task GetPermissionsForUser_ShouldReturnPermissionsFromCacheSecondTime()
        {
            await _dut.GetPermissionsForUserAsync(TestPlant, Oid);
            // Act
            var result = await _dut.GetPermissionsForUserAsync(TestPlant, Oid);

            // Assert
            AssertPermissions(result);
            // since GetPermissionsForUserAsync has been called twice, but GetPermissionsAsync has been called once, the second Get uses cache
            _permissionApiServiceMock.Verify(p => p.GetPermissionsAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task GetProjectsForUserAsync_ShouldReturnProjectsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetProjectsForUserAsync(TestPlant, Oid);

            // Assert
            AssertProjects(result);
            _permissionApiServiceMock.Verify(p => p.GetAllOpenProjectsAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task GetProjectsForUserAsync_ShouldReturnProjectsFromCacheSecondTime()
        {
            await _dut.GetProjectsForUserAsync(TestPlant, Oid);
            // Act
            var result = await _dut.GetProjectsForUserAsync(TestPlant, Oid);

            // Assert
            AssertProjects(result);
            // since GetProjectsForUserAsync has been called twice, but GetProjectsAsync has been called once, the second Get uses cache
            _permissionApiServiceMock.Verify(p => p.GetAllOpenProjectsAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task GetPermissionsForUser_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetPermissionsForUserAsync(TestPlant, Guid.Empty));

        [TestMethod]
        public async Task GetProjectsForUserAsync_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetProjectsForUserAsync(TestPlant, Guid.Empty));

        [TestMethod]
        public void ClearAll_ShouldClearAllPermissionCaches()
        {
            // Arrange
            var cacheManagerMock = new Mock<ICacheManager>();
            var dut = new PermissionCache(
                cacheManagerMock.Object,
                _permissionApiServiceMock.Object,
                new Mock<IOptionsMonitor<CacheOptions>>().Object);
            // Act
            dut.ClearAll(TestPlant, Oid);

            // Assert
            cacheManagerMock.Verify(c => c.Remove(It.IsAny<string>()), Times.Exactly(2));
        }

        private void AssertPermissions(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Permission1, result.First());
            Assert.AreEqual(Permission2, result.Last());
        }

        private void AssertProjects(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Project1WithAccess, result.First());
            Assert.AreEqual(Project2WithAccess, result.Last());
        }
    }
}
