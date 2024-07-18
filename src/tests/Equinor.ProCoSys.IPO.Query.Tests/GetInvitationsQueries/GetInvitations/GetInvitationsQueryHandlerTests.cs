using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsQueries.GetInvitations
{
    [TestClass]
    public class GetInvitationsQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private Invitation _invitation1;
        private Invitation _invitation2;
        private Invitation _invitation3;

        private string _functionalRoleCode1 = "FrCode1";
        private string _functionalRoleCode2 = "FrCode2";
        private string _functionalRoleCode3 = "FrCode3";
        private readonly Guid _personGuid = new Guid("11111111-2222-2222-2222-333333333333");
        private readonly Guid _frPersonGuid1 = new Guid("11111111-2222-2222-2222-333333333332"); 
        private readonly Guid _frPersonGuid2 = new Guid("11111111-2222-2222-2222-333333333335"); 
        private readonly Guid _frPersonGuid3 = new Guid("11111111-2222-2222-2222-333333333336"); 
        private string _frEmail1 = "FR1@email.com";
        private string _personEmail1 = "P1@email.com";
        private string _personEmail2 = "P2@email.com";
        private string _personEmail3 = "P3@email.com";
        private string _externalPersonEmail = "P4@email.com";
        private string _frPersonEmail1 = "frp1@email.com";
        private string _frPersonEmail2 = "frp2@email.com";
        private string _frPersonEmail3 = "frp3@email.com";
        const string _projectName = "Project1";
        const string _projectName2 = "Project2";
        const string _title1 = "Title1";
        const string _title2 = "Title2";
        const string _commPkgNo = "CommPkgNo";
        const string _commPkgNo2 = "CommPkgNo2";
        const string _mcPkgNo = "McPkgNo";
        private const string _system = "1|2";
        private const int _projectId1 = 320;
        private const int _projectId2 = 640;
        private static readonly Guid _project1Guid = new Guid("11111111-2222-2222-2222-333333333341");
        private static readonly Guid _project2Guid = new Guid("11111111-2222-2222-2222-333333333342");


        private readonly Project _project1 = new(TestPlant, _projectName, $"Description of {_projectName}", _project1Guid);
        private readonly Project _project2 = new(TestPlant, _projectName2, $"Description of {_projectName2}", _project2Guid);

        private IFunctionalRoleApiService _functionalRoleApiService;


        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _project1.SetProtectedIdForTesting(_projectId1);
                _project2.SetProtectedIdForTesting(_projectId2);

                const string description = "Description";

                var contractorFunctionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    _functionalRoleCode1,
                    null,
                    null,
                    null,
                    _frEmail1,
                    null,
                    0);

                var constructionCompanyFunctionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.FunctionalRole,
                    _functionalRoleCode2,
                    null,
                    null,
                    null,
                    null,
                    null,
                    1);

                var commissioningFunctionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.Commissioning,
                    IpoParticipantType.FunctionalRole,
                    _functionalRoleCode3,
                    null,
                    null,
                    null,
                    null,
                    null,
                    2);

                var contractorPersonParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "FirstName",
                    "LastName",
                    "UN",
                    _personEmail1,
                    CurrentUserOid,
                    0);

                var constructionCompanyPersonParticipant = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "FirstName2",
                    "LastName2",
                    "UN",
                    _personEmail2,
                    _personGuid,
                    1);

                var contractorPersonParticipant1 = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "FirstName1",
                    "LastName1",
                    "UN",
                    _personEmail1,
                    CurrentUserOid,
                    0);

                var constructionCompanyPersonParticipant1 = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "FirstName",
                    "LastName",
                    "UN",
                    _personEmail2,
                    _personGuid,
                    1);

                var commissioningPersonParticipant = new Participant(
                    TestPlant,
                    Organization.Commissioning,
                    IpoParticipantType.Person,
                    null,
                    "FirstName3",
                    "LastName3",
                    "UN",
                    _personEmail3,
                    _personGuid,
                    7);

                var externalPersonParticipant = new Participant(
                    TestPlant,
                    Organization.External,
                    IpoParticipantType.Person,
                    null,
                    null,
                    null,
                    null,
                    _externalPersonEmail,
                    _personGuid,
                    8);

                var contractorFunctionalRolePersonParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    _functionalRoleCode1,
                    "FirstName2",
                    "LastName2",
                    "UN2",
                    _frPersonEmail1,
                    _frPersonGuid1,
                    0);

                var constructionCompanyFunctionalRolePersonParticipant = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    _functionalRoleCode2,
                    "FirstName3",
                    "LastName3",
                    "UN3",
                    _frPersonEmail2,
                    _frPersonGuid2,
                    10);

                var constructionCompanyFunctionalRolePersonParticipant1 = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    _functionalRoleCode3,
                    "FirstName4",
                    "LastName4",
                    "UN4",
                    _frPersonEmail3,
                    _frPersonGuid3,
                    11);

                var commPkg = new CommPkg(
                    TestPlant,
                    _project1,
                    _commPkgNo,
                    description,
                    "OK",
                    "1|2",
                    Guid.Empty);

                var mcPkg1 = new McPkg(
                    TestPlant,
                    _project1,
                    _commPkgNo,
                    _mcPkgNo,
                    description,
                    _system,
                    Guid.Empty,
                    Guid.Empty);

                var mcPkg2 = new McPkg(
                    TestPlant,
                    _project1,
                    _commPkgNo2,
                    _mcPkgNo,
                    description,
                    _system,
                    Guid.Empty,
                    Guid.Empty);

                var startTime1 = _timeProvider.UtcNow;

                _invitation1 = new Invitation(
                    TestPlant,
                    _project1,
                    _title1,
                    "Description",
                    DisciplineType.DP,
                    startTime1,
                    startTime1.AddHours(1),
                    null,
                    new List<McPkg> { mcPkg1 },
                    null);

                _invitation1.AddParticipant(contractorFunctionalRoleParticipant);
                _invitation1.AddParticipant(constructionCompanyPersonParticipant);
                _invitation1.AddParticipant(contractorFunctionalRolePersonParticipant);
                _invitation1.AddParticipant(commissioningPersonParticipant);
                _invitation1.AddParticipant(commissioningFunctionalRoleParticipant);
                _invitation1.AddParticipant(constructionCompanyFunctionalRolePersonParticipant1);
                _invitation1.AddParticipant(externalPersonParticipant);

                var startTime2 = _timeProvider.UtcNow.AddWeeks(1);

                _invitation2 = new Invitation(
                    TestPlant,
                    _project2,
                    _title1,
                    "Description",
                    DisciplineType.MDP,
                    startTime2,
                    startTime2.AddHours(1),
                    null,
                    null,
                    new List<CommPkg> { commPkg });

                _invitation2.AddParticipant(constructionCompanyFunctionalRoleParticipant);
                _invitation2.AddParticipant(contractorPersonParticipant);
                _invitation2.AddParticipant(constructionCompanyFunctionalRolePersonParticipant);

                var startTime3 = _timeProvider.UtcNow.AddWeeks(2);

                _invitation3 = new Invitation(
                    TestPlant,
                    _project2,
                    _title2,
                    "Description",
                    DisciplineType.DP,
                    startTime3,
                    startTime3.AddHours(1),
                    null,
                    new List<McPkg> {mcPkg2},
                    null);

                _invitation3.AddParticipant(contractorPersonParticipant1);
                _invitation3.AddParticipant(constructionCompanyPersonParticipant1);

                context.Projects.Add(_project1);
                context.Projects.Add(_project2);
                context.Invitations.Add(_invitation1);
                context.Invitations.Add(_invitation2);
                context.Invitations.Add(_invitation3);
                context.SaveChangesAsync().Wait();
            }
            var _functionalRole1Details = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleCode1,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var _functionalRole2Details = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleCode2,
                Description = "FR description",
                Email = "fr@email.com;fr2@email.com",
                InformationEmail = null,
                Persons = new List<ProCoSysPerson>
                {
                    new ProCoSysPerson{
                        AzureOid = new Guid("11111111-1111-2222-2222-333333333333").ToString(),
                        Email = "ola@test.com",
                        FirstName = "Ola",
                        LastName = "Nordmann",
                        UserName = "ON"
                    }
                },
                UsePersonalEmail = false
            };
            var _functionalRole3Details = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleCode3,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = "ie@email.com;ie2@email.com",
                Persons = null,
                UsePersonalEmail = false
            };


            IList<ProCoSysFunctionalRole> fr1Details = new List<ProCoSysFunctionalRole> { _functionalRole1Details };
            IList<ProCoSysFunctionalRole> fr2Details = new List<ProCoSysFunctionalRole> { _functionalRole2Details };
            IList<ProCoSysFunctionalRole> fr13Details = new List<ProCoSysFunctionalRole> { _functionalRole1Details, _functionalRole2Details, _functionalRole3Details };


            var _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(TestPlant, new List<string> { _functionalRoleCode2 }))
                .Returns(Task.FromResult(fr2Details));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(TestPlant, new List<string> { _functionalRoleCode1, _functionalRoleCode3 }))
                .Returns(Task.FromResult(fr13Details));

            _functionalRoleApiService = _functionalRoleApiServiceMock.Object;

        }

        [TestMethod]
        public async Task Handler_ShouldReturnOkResult()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);

                Assert.IsNotNull(result);
                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectNumberOfInvitations_NoFilter()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);

                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnPageSize()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, null, new Paging(1, 1));
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);

                var invitationResults = result.Data;
                Assert.AreEqual(2, invitationResults.MaxAvailable);
                Assert.AreEqual(1, invitationResults.Invitations.Count());
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnEmptyPageButMaxAvailable_WhenGettingBehindLastPage()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, null, new Paging(1, 50));
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(2, result.Data.MaxAvailable);
                Assert.AreEqual(0, result.Data.Invitations.Count());
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnInvitationsByStartTime_WhenSortingAsc()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, new Sorting(SortingDirection.Asc, SortingProperty.PunchOutDateUtc));
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.Last().Id);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnInvitationsByStartTime_WhenSortingDesc()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, new Sorting(SortingDirection.Desc, SortingProperty.PunchOutDateUtc));
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.Last().Id);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnId()
        {
            var filter = new Filter {IpoIdStartsWith = "IPO-2"};

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnStatusAccepted()
        {
            var filter = new Filter { IpoStatuses = new List<IpoStatus> {IpoStatus.Accepted} };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnStatusPlanned()
        {
            var filter = new Filter { IpoStatuses = new List<IpoStatus> { IpoStatus.Planned } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnStatusScopeHandedOver()
        {
            var filter = new Filter { IpoStatuses = new List<IpoStatus> { IpoStatus.ScopeHandedOver } };

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var inv = context.Invitations.First(i => i.Status == IpoStatus.Planned);
                inv.ScopeHandedOver();
                context.SaveChangesAsync().Wait();

                var query = new GetInvitationsQuery(_projectName, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutDate_FromNow()
        {
            var filter = new Filter { PunchOutDateFromUtc = _timeProvider.UtcNow };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutDate_FromTwoWeeksInFuture()
        {
            var filter = new Filter { PunchOutDateFromUtc = _timeProvider.UtcNow.AddWeeks(2) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPersonOid()
        {
            var filter = new Filter { PersonOid = _frPersonGuid2 };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnFunctionalRoleCode()
        {
            var filter = new Filter { FunctionalRoleCode = _functionalRoleCode2 };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnCommPkgNoStartWith_ShouldGetOneInvitationWithMcPkgWithCommPkgParent()
        {
            var filter = new Filter { CommPkgNoStartsWith = _commPkgNo2 };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnCommPkgNoStartWith_ShouldGetInvitaionsWithMcPkgAndInvitationWithCommpkg()
        {
            var filter = new Filter { CommPkgNoStartsWith = _commPkgNo };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnMcPkgNoStartWith()
        {
            var filter = new Filter { McPkgNoStartsWith = _mcPkgNo };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutOverdue_NoInvitations()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.Overdue } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutOverdue_TwoInvitations()
        {
            _timeProvider.ElapseWeeks(4);
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.Overdue } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutInNextWeek_OneInvitation()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.NextWeek } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data,1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutInNextWeek_ElapseTime_NoInvitations()
        {
            _timeProvider.ElapseWeeks(-1);
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.NextWeek } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutInThisWeek_NoInvitation()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.ThisWeek } };
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }
        
        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutInThisWeek_ElapseWeek_OneInvitation()
        {
            _timeProvider.ElapseWeeks(1);
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.ThisWeek } };
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtFromNow_TwoInvitations()
        {
            var filter = new Filter { LastChangedAtFromUtc = _timeProvider.UtcNow };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtFromThreeWeeks_NoInvitations()
        {
            var filter = new Filter { LastChangedAtFromUtc = _timeProvider.UtcNow.AddWeeks(3) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtToLastWeek_NoInvitations()
        {
            var filter = new Filter { LastChangedAtToUtc = _timeProvider.UtcNow.AddWeeks(-1) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtToThreeWeeks_TwoInvitations()
        {
            var filter = new Filter { LastChangedAtToUtc = _timeProvider.UtcNow.AddWeeks(3) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldSortOnContractorRep_Asc()
        {
            var sorting = new Sorting(SortingDirection.Asc, SortingProperty.ContractorRep);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, sorting);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldSortOnContractorRep_Desc()
        {
            var sorting = new Sorting(SortingDirection.Desc, SortingProperty.ContractorRep);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, sorting);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldSortOnType_Asc()
        {
            var sorting = new Sorting(SortingDirection.Asc, SortingProperty.Type);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, sorting);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldSortOnType_Desc()
        {
            var sorting = new Sorting(SortingDirection.Desc, SortingProperty.Type);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, sorting);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldSeparateOutAdditionalConstructionCompanyParticipants()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);

                var invitationDto = result.Data.Invitations.First();
                var invitation = _invitation1;

                var firstConstructionCompanyRep = invitation.Participants.Single(p =>
                    p.Organization == Organization.ConstructionCompany && p.SortKey == 1);
                Assert.IsTrue(invitationDto.ConstructionCompanyRep.StartsWith(firstConstructionCompanyRep.FirstName));
                Assert.IsFalse(invitationDto.AdditionalConstructionCompanyReps
                    .Any(p => p.StartsWith(firstConstructionCompanyRep.FirstName)));
                Assert.AreEqual(invitationDto.AdditionalConstructionCompanyReps.Count(), 
                    invitation.Participants.Count(p => p.Organization == Organization.ConstructionCompany && p.SortKey != 1));
            }
        }

        [TestMethod]
        public async Task Handler_ShouldNotShowParticipants_WhenUsingPersonalEmail()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider,
                    _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);

                var invitationDto = result.Data.Invitations.First();

                Assert.AreEqual(invitationDto.AdditionalConstructionCompanyReps.Count(), 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectDto()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);

                var invitationDto = result.Data.Invitations.First();
                var invitation = _invitation1;
                Assert.AreEqual(0, invitationDto.CommPkgNos.Count());
                Assert.AreEqual(GetProjectById(invitation.ProjectId).Name, invitationDto.ProjectName);
                Assert.AreEqual(invitation.Title, invitationDto.Title);
                Assert.AreEqual(invitation.Description, invitationDto.Description);
                Assert.AreEqual(invitation.Id, invitationDto.Id);
                Assert.AreEqual(invitation.Location, invitationDto.Location);
                Assert.AreEqual(invitation.Type, invitationDto.Type);
                Assert.AreEqual(
                    invitation.Participants.Single(p => p.SortKey == 0 && p.Type == IpoParticipantType.FunctionalRole)
                        .FunctionalRoleCode, invitationDto.ContractorRep);
                Assert.IsTrue(invitationDto.ConstructionCompanyRep.StartsWith(invitation.Participants.Single(p => p.SortKey == 1).FirstName));
                Assert.IsTrue(invitationDto.CommissioningReps.Any(p => p.StartsWith(invitation.Participants.First(part => part.Organization == Organization.Commissioning).FirstName)));
                Assert.AreEqual(invitationDto.CommissioningReps.Count(), invitation.Participants.Count(p => p.Organization == Organization.Commissioning));
                Assert.IsTrue(invitationDto.ExternalGuests.Any(p => p.StartsWith(invitation.Participants.First(part => part.Organization == Organization.External).Email)));
                Assert.AreEqual(invitationDto.ExternalGuests.Count(), invitation.Participants.Count(p => p.Organization == Organization.External));
                Assert.AreEqual(invitation.McPkgs.Single().McPkgNo, invitationDto.McPkgNos.Single());
                Assert.AreEqual(invitation.Status, invitationDto.Status);
                Assert.AreEqual(invitation.EndTimeUtc, invitationDto.EndTimeUtc);
                Assert.AreEqual(invitation.StartTimeUtc, invitationDto.StartTimeUtc);
                Assert.AreEqual(invitation.CompletedAtUtc, invitationDto.CompletedAtUtc);
                Assert.AreEqual(invitation.AcceptedAtUtc, invitationDto.AcceptedAtUtc);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnMoreThanOneProjectName_When_RequestProjectName_Is_Null()
        {
            var filter = new Filter { };
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(null, null, filter);
                var dut = new GetInvitationsQueryHandler(context, _permissionCache, _plantProvider, _currentUserProvider, _functionalRoleApiService);

                var result = await dut.Handle(query, default);
                _permissionCacheMock.Verify(x => x.GetProjectsForUserAsync(_plantProvider.Plant, CurrentUserOid), Times.Once);                
                Assert.IsTrue(result.Data.Invitations.DistinctBy(x => x.ProjectName).Count() > 1);
            }
        }

        private void AssertCount(InvitationsResult data, int count)
        {
            Assert.AreEqual(count, data.MaxAvailable);
            Assert.AreEqual(count, data.Invitations.Count());
        }
    }
}
