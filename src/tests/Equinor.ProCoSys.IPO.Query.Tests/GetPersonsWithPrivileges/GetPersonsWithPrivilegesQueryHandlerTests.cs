using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetPersons;
using Equinor.ProCoSys.IPO.Query.GetPersonsWithPrivileges;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetPersonsWithPrivileges
{
    [TestClass]
    public class GetPersonsWithPrivilegesQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IPersonApiService> _personApiServiceMock;
        private IList<ProCoSysPerson> _mainApiContractorPersons;
        private IList<ProCoSysPerson> _mainApiConstructionPersons;
        private GetPersonsWithPrivilegesQuery _query;

        private readonly string _objectName = "IPO";
        private List<string> _privileges = new List<string> { "SIGN", "CREATE" };
        private readonly string _searchString = "A";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _personApiServiceMock = new Mock<IPersonApiService>();
                _mainApiContractorPersons = new List<ProCoSysPerson>
                {
                    new ProCoSysPerson
                    {
                        AzureOid = "12345678-1234-123456789123",
                        FirstName = "F1",
                        LastName = "L1",
                        UserName = "U1",
                        Email = "E1"
                    },
                    new ProCoSysPerson
                    {
                        AzureOid = "12345678-1235-123456789123",
                        FirstName = "F2",
                        LastName = "L2",
                        UserName = "U2",
                        Email = "E2"
                    },
                    new ProCoSysPerson
                    {
                        AzureOid = "12345678-1236-123456789123",
                        FirstName = "F3",
                        LastName = "L3",
                        UserName = "U3",
                        Email = "E3"
                    }
                };

                _personApiServiceMock = new Mock<IPersonApiService>();
                _mainApiConstructionPersons = new List<ProCoSysPerson>
                {
                    new ProCoSysPerson
                    {
                        AzureOid = "12345678-1237-123456789123",
                        FirstName = "FN4",
                        LastName = "LN4",
                        UserName = "UN4",
                        Email = "EM4"
                    },
                    new ProCoSysPerson
                    {
                        AzureOid = "12345678-1238-123456789123",
                        FirstName = "FN5",
                        LastName = "LN5",
                        UserName = "UN5",
                        Email = "EM5"
                    },
                    new ProCoSysPerson
                    {
                        AzureOid = "12345678-1239-123456789123",
                        FirstName = "FN6",
                        LastName = "LN6",
                        UserName = "UN6",
                        Email = "EM6"
                    }
                };

                _query = new GetPersonsWithPrivilegesQuery(_searchString, _objectName, _privileges);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetPersonsWithPrivilegesQueryHandler(_personApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task HandleGetContractorPersons_ShouldReturnCorrectItems()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _personApiServiceMock
                    .Setup(x => x.GetPersonsWithPrivilegesAsync(TestPlant, _searchString, _objectName, _privileges))
                    .Returns(Task.FromResult(_mainApiContractorPersons));

                _query = new GetPersonsWithPrivilegesQuery(_searchString, _objectName, _privileges);

                var dut = new GetPersonsWithPrivilegesQueryHandler(_personApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var person1 = result.Data.ElementAt(0);
                var person2 = result.Data.ElementAt(1);
                var person3 = result.Data.ElementAt(2);
                AssertPersonData(_mainApiContractorPersons.Single(c => c.AzureOid == person1.AzureOid), person1);
                AssertPersonData(_mainApiContractorPersons.Single(t => t.AzureOid == person2.AzureOid), person2);
                AssertPersonData(_mainApiContractorPersons.Single(t => t.AzureOid == person3.AzureOid), person3);
            }
        }

        [TestMethod]
        public async Task HandleGetContractorPersons_ShouldReturnEmptyList_WhenSearchReturnsNull()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _query = new GetPersonsWithPrivilegesQuery(_searchString, _objectName, _privileges);

                var dut = new GetPersonsWithPrivilegesQueryHandler(_personApiServiceMock.Object, _plantProvider);
                _personApiServiceMock
                    .Setup(x => x.GetPersonsWithPrivilegesAsync(TestPlant, _searchString, _objectName, _privileges))
                    .Returns(Task.FromResult<IList<ProCoSysPerson>>(null));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        [TestMethod]
        public async Task HandleGetConstructionPersons_ShouldReturnCorrectItems()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _personApiServiceMock
                    .Setup(x => x.GetPersonsWithPrivilegesAsync(TestPlant, _searchString, _objectName, _privileges))
                    .Returns(Task.FromResult(_mainApiConstructionPersons));

                _query = new GetPersonsWithPrivilegesQuery(_searchString, _objectName, _privileges);

                var dut = new GetPersonsWithPrivilegesQueryHandler(_personApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var person1 = result.Data.ElementAt(0);
                var person2 = result.Data.ElementAt(1);
                var person3 = result.Data.ElementAt(2);
                AssertPersonData(_mainApiConstructionPersons.Single(c => c.AzureOid == person1.AzureOid), person1);
                AssertPersonData(_mainApiConstructionPersons.Single(t => t.AzureOid == person2.AzureOid), person2);
                AssertPersonData(_mainApiConstructionPersons.Single(t => t.AzureOid == person3.AzureOid), person3);
            }
        }

        [TestMethod]
        public async Task HandleGetConstructionPersons_ShouldReturnEmptyList_WhenSearchReturnsNull()
        {
            _query = new GetPersonsWithPrivilegesQuery(_searchString, _objectName, _privileges);

            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetPersonsWithPrivilegesQueryHandler(_personApiServiceMock.Object, _plantProvider);
                _personApiServiceMock
                    .Setup(x => x.GetPersonsWithPrivilegesAsync(TestPlant, _searchString, _objectName, _privileges))
                    .Returns(Task.FromResult<IList<ProCoSysPerson>>(null));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private void AssertPersonData(ProCoSysPerson PCSPerson, ProCoSysPersonDto personDto)
        {
            Assert.AreEqual(PCSPerson.AzureOid, personDto.AzureOid);
            Assert.AreEqual(PCSPerson.FirstName, personDto.FirstName);
            Assert.AreEqual(PCSPerson.LastName, personDto.LastName);
            Assert.AreEqual(PCSPerson.UserName, personDto.UserName);
            Assert.AreEqual(PCSPerson.Email, personDto.Email);
        }
    }
}
