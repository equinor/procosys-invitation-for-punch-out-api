using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetFunctionalRoles;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetFunctionalRoles
{
    [TestClass]
    public class GetFunctionalRolesForIpoQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;
        private IList<ProCoSysFunctionalRole> _libraryApiFunctionalRoles;
        private GetFunctionalRolesForIpoQuery _query;

        private Person person;
        private const string _classification = "Classification";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                person = new Person() { AzureOid = "123456", FirstName = "F1", LastName = "F2", UserName = "UN1", Email = "E1@email.com" };

                _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
                _libraryApiFunctionalRoles = new List<ProCoSysFunctionalRole>
                {
                    new ProCoSysFunctionalRole()
                    {
                        Code = "C1", Description = "D1", Email = "e1@email.com", InformationEmail = "ie1@email.com", UsePersonalEmail = true
                    },
                    new ProCoSysFunctionalRole
                    {
                        Code = "C2", Description = "D2", Email = "e2@email.com", InformationEmail = "ie1@email.com", UsePersonalEmail = false, Persons = new List<Person>() { person }
                    }
                };

                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByClassificationAsync(TestPlant, _classification))
                    .Returns(Task.FromResult(_libraryApiFunctionalRoles));

                _query = new GetFunctionalRolesForIpoQuery(_classification);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetFunctionalRolesForIpoQueryHandler(_functionalRoleApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetFunctionalRolesForIpoQueryHandler(_functionalRoleApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(2, result.Data.Count);
                var item1 = result.Data.ElementAt(0);
                var item2 = result.Data.ElementAt(1);
                AssertFunctionalRoleData(_libraryApiFunctionalRoles.Single(c => c.Code == item1.Code), item1);
                Assert.IsTrue(item1.UsePersonalEmail != null && item1.UsePersonalEmail.Value);
                AssertFunctionalRoleData(_libraryApiFunctionalRoles.Single(t => t.Code == item2.Code), item2);
                Assert.IsFalse(item2.UsePersonalEmail != null && item2.UsePersonalEmail.Value);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnEmptyList_WhenReturnsNull()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetFunctionalRolesForIpoQueryHandler(_functionalRoleApiServiceMock.Object, _plantProvider);
                _functionalRoleApiServiceMock
                    .Setup(x => x.GetFunctionalRolesByClassificationAsync(TestPlant, _classification))
                    .Returns(Task.FromResult<IList<ProCoSysFunctionalRole>>(null));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private void AssertFunctionalRoleData(ProCoSysFunctionalRole pcsFunctionalRole, ProCoSysFunctionalRoleDto functionalRoleDto)
        {
            Assert.AreEqual(pcsFunctionalRole.Code, functionalRoleDto.Code);
            Assert.AreEqual(pcsFunctionalRole.Description, functionalRoleDto.Description);
            Assert.AreEqual(pcsFunctionalRole.Email, functionalRoleDto.Email);
            Assert.AreEqual(pcsFunctionalRole.InformationEmail, functionalRoleDto.InformationEmail);
            Assert.AreEqual(pcsFunctionalRole.Persons, functionalRoleDto.Persons);
        }
    }
}
