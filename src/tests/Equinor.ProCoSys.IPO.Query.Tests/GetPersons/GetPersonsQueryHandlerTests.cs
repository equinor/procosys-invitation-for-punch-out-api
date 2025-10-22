using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetPersons;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetPersons
{
    [TestClass]
    public class GetPersonsQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private IList<ProCoSysPerson> _mainApiPersons;
        private GetPersonsQuery _query;

        private readonly string _searchString = "A";
        private readonly long _numOfRows = 1000;

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _personApiServiceMock = new Mock<IPersonApiService>();
                _mainApiPersons = new List<ProCoSysPerson>
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

                _query = new GetPersonsQuery(_searchString);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetPersonsQueryHandler(_personApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task HandleGetPersons_ShouldReturnCorrectItems()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _personApiServiceMock
                    .Setup(x => x.GetPersonsAsync(TestPlant, _searchString, It.IsAny<CancellationToken>(), _numOfRows))
                    .Returns(Task.FromResult(_mainApiPersons));

                _query = new GetPersonsQuery(_searchString);

                var dut = new GetPersonsQueryHandler(_personApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var person1 = result.Data.ElementAt(0);
                var person2 = result.Data.ElementAt(1);
                var person3 = result.Data.ElementAt(2);
                AssertPersonData(_mainApiPersons.Single(c => c.AzureOid == person1.AzureOid), person1);
                AssertPersonData(_mainApiPersons.Single(t => t.AzureOid == person2.AzureOid), person2);
                AssertPersonData(_mainApiPersons.Single(t => t.AzureOid == person3.AzureOid), person3);
            }
        }

        [TestMethod]
        public async Task HandleGetContractorPersons_ShouldReturnEmptyList_WhenSearchReturnsNull()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _query = new GetPersonsQuery(_searchString);

                var dut = new GetPersonsQueryHandler(_personApiServiceMock.Object, _plantProvider);
                _personApiServiceMock
                    .Setup(x => x.GetPersonsAsync(TestPlant, _searchString, It.IsAny<CancellationToken>(), _numOfRows))
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
