﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetPersons;
using Equinor.ProCoSys.IPO.Query.GetPersonsInUserGroup;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetPersonsInUserGroup
{
    [TestClass]
    public class GetPersonsInUserGroupQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IPersonApiService> _personApiServiceMock;
        private IList<ProCoSysPerson> _mainApiContractorPersons;
        private IList<ProCoSysPerson> _mainApiConstructionPersons;
        private GetPersonsInUserGroupQuery _query;

        private readonly string _contractorUserGroup = "MC_CONTRACTOR_MLA";
        private readonly string _constructionUserGroup = "MC_LEAD_DISCIPLINE";
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

                _query = new GetPersonsInUserGroupQuery(_searchString, _contractorUserGroup);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetPersonsInUserGroupQueryHandler(_personApiServiceMock.Object, _plantProvider);
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
                    .Setup(x => x.GetPersonsByUserGroupAsync(TestPlant, _searchString, _contractorUserGroup))
                    .Returns(Task.FromResult(_mainApiContractorPersons));

                _query = new GetPersonsInUserGroupQuery(_searchString, _contractorUserGroup);

                var dut = new GetPersonsInUserGroupQueryHandler(_personApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var person1 = result.Data.ElementAt(0);
                var person2 = result.Data.ElementAt(1);
                var person3 = result.Data.ElementAt(2);
                AssertPersonData(_mainApiContractorPersons.Single(c => c.AzureOid == person1.Oid), person1);
                AssertPersonData(_mainApiContractorPersons.Single(t => t.AzureOid == person2.Oid), person2);
                AssertPersonData(_mainApiContractorPersons.Single(t => t.AzureOid == person3.Oid), person3);
            }
        }

        [TestMethod]
        public async Task HandleGetContractorPersons_ShouldReturnEmptyList_WhenSearchReturnsNull()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _query = new GetPersonsInUserGroupQuery(_searchString, _contractorUserGroup);

                var dut = new GetPersonsInUserGroupQueryHandler(_personApiServiceMock.Object, _plantProvider);
                _personApiServiceMock
                    .Setup(x => x.GetPersonsByUserGroupAsync(TestPlant, _searchString, _contractorUserGroup))
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
                    .Setup(x => x.GetPersonsByUserGroupAsync(TestPlant, _searchString, _constructionUserGroup))
                    .Returns(Task.FromResult(_mainApiConstructionPersons));

                _query = new GetPersonsInUserGroupQuery(_searchString, _constructionUserGroup);

                var dut = new GetPersonsInUserGroupQueryHandler(_personApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var person1 = result.Data.ElementAt(0);
                var person2 = result.Data.ElementAt(1);
                var person3 = result.Data.ElementAt(2);
                AssertPersonData(_mainApiConstructionPersons.Single(c => c.AzureOid == person1.Oid), person1);
                AssertPersonData(_mainApiConstructionPersons.Single(t => t.AzureOid == person2.Oid), person2);
                AssertPersonData(_mainApiConstructionPersons.Single(t => t.AzureOid == person3.Oid), person3);
            }
        }

        [TestMethod]
        public async Task HandleGetConstructionPersons_ShouldReturnEmptyList_WhenSearchReturnsNull()
        {
            _query = new GetPersonsInUserGroupQuery(_searchString, _constructionUserGroup);

            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetPersonsInUserGroupQueryHandler(_personApiServiceMock.Object, _plantProvider);
                _personApiServiceMock
                    .Setup(x => x.GetPersonsByUserGroupAsync(TestPlant, _searchString, _constructionUserGroup))
                    .Returns(Task.FromResult<IList<ProCoSysPerson>>(null));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private void AssertPersonData(ProCoSysPerson PCSPerson, ProCoSysPersonDto personDto)
        {
            Assert.AreEqual(PCSPerson.AzureOid, personDto.Oid);
            Assert.AreEqual(PCSPerson.FirstName, personDto.FirstName);
            Assert.AreEqual(PCSPerson.LastName, personDto.LastName);
            Assert.AreEqual(PCSPerson.UserName, personDto.UserName);
            Assert.AreEqual(PCSPerson.Email, personDto.Email);
        }
    }
}