using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.ExportIPOs;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitationsQueries.GetInvitationsForExport
{
    [TestClass]
    public class GetInvitationsForExportQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private ILogger<GetInvitationsForExportQuery> _logger;
        private IExportIpoRepository _exportIpoRepository;

        private Invitation _invitation1;
        private Invitation _invitation2;
        private Invitation _invitation3;
        private string _functionalRoleCode1 = "FrCode1";
        private string _functionalRoleCode2 = "FrCode2";
        private readonly Guid _personGuid = new Guid("11111111-2222-2222-2222-333333333333");
        private readonly Guid _frPersonGuid1 = new Guid("11111111-2222-2222-2222-333333333332");
        private readonly Guid _frPersonGuid2 = new Guid("11111111-2222-2222-2222-333333333335");
        private static readonly Guid _project1Guid = new Guid("11111111-2222-2222-2222-333333333341");
        private static readonly Guid _project2Guid = new Guid("11111111-2222-2222-2222-333333333342");
        private string _frEmail1 = "FR1@email.com";
        private string _personEmail1 = "P1@email.com";
        private string _personEmail2 = "P2@email.com";
        private string _personEmail3 = "P3@email.com";
        private string _frPersonEmail1 = "frp1@email.com";
        private string _frPersonEmail2 = "frp2@email.com";
        const string _projectName = "Project1";
        const string _projectName2 = "Project2";
        const string _title1 = "Title1";
        const string _title2 = "Title2";
        const string _commPkgNo = "CommPkgNo";
        const string _commPkgNo2 = "CommPkgNo2";
        const string _mcPkgNo = "McPkgNo";
        const string _system = "1|2";
        private readonly Project _project1 = new(TestPlant, _projectName, $"Description of {_projectName}", _project1Guid);
        private readonly Project _project2 = new(TestPlant, _projectName2, $"Description of {_projectName2}" , _project2Guid);

        [TestInitialize]
        public void Setup()
        {
            _logger = new Mock<ILogger<GetInvitationsForExportQuery>>().Object;
            var exportIpoRepositoryMock = new Mock<IExportIpoRepository>();

            // InMemory database used for unit test does not support stored procedures. Hence we are mocking return results from this.
            exportIpoRepositoryMock
                .Setup(x => x.GetInvitationsWithIncludesAsync(It.IsAny<List<int>>(), _plantProvider, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Invitation>(){_invitation1,_invitation2,_invitation3}));
                
            _exportIpoRepository = exportIpoRepositoryMock.Object;
        }

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context =
                new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                context.Projects.Add(_project1);
                context.Projects.Add(_project2);

                const string description = "Description";

                var functionalRoleParticipant1 = new Participant(
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

                var functionalRoleParticipant2 = new Participant(
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

                var personParticipant1 = new Participant(
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

                var personParticipant2 = new Participant(
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

                var personParticipant3 = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "FirstName3",
                    "LastName3",
                    "UN",
                    _personEmail1,
                    CurrentUserOid,
                    0);

                var personParticipant4 = new Participant(
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

                var frPerson1 = new Participant(
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

                var frPerson2 = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    _functionalRoleCode2,
                    "FirstName3",
                    "LastName3",
                    "UN3",
                    _frPersonEmail2,
                    _frPersonGuid2,
                    1);

                var supplierPersonParticipant = new Participant(
                    TestPlant,
                    Organization.Supplier,
                    IpoParticipantType.Person,
                    null,
                    "SupplierFirstName",
                    "SupplierLastName",
                    "UN",
                    _personEmail3,
                    _personGuid,
                    5);

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
                    new List<McPkg> {mcPkg1},
                    null);

                _invitation1.AddParticipant(functionalRoleParticipant1);
                _invitation1.AddParticipant(personParticipant2);
                _invitation1.AddParticipant(frPerson1);
                _invitation1.AddParticipant(supplierPersonParticipant);

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

                _invitation2.AddParticipant(functionalRoleParticipant2);
                _invitation2.AddParticipant(personParticipant1);
                _invitation2.AddParticipant(frPerson2);

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
                    new List<McPkg> { mcPkg2 },
                    null);

                _invitation3.AddParticipant(personParticipant3);
                _invitation3.AddParticipant(personParticipant4);

                context.Invitations.Add(_invitation1);
                var history1 = new History(TestPlant, "D1", _invitation1.Guid, EventType.IpoCreated);
                context.History.Add(history1);

                context.Invitations.Add(_invitation2);
                var history2 = new History(TestPlant, "D2", _invitation2.Guid, EventType.IpoCreated);
                context.History.Add(history2);

                context.Invitations.Add(_invitation3);
                var history3 = new History(TestPlant, "D3", _invitation3.Guid, EventType.IpoCreated);
                context.History.Add(history3);
                
                context.SaveChangesAsync().Wait();
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldReturnOkResult()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);
                var query = new GetInvitationsForExportQuery(_projectName);

                var result = await dut.Handle(query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldReturnCorrectNumberOfInvitations_NoFilter()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);

                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldReturnInvitationsByStartTime_WhenSortingAsc()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, new Sorting(SortingDirection.Asc, SortingProperty.PunchOutDateUtc));
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.Last().Id);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldReturnInvitationsByStartTime_WhenSortingDesc()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, new Sorting(SortingDirection.Desc, SortingProperty.PunchOutDateUtc));
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.Last().Id);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnId()
        {
            var filter = new Filter { IpoIdStartsWith = "IPO-2" };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnStatusAccepted()
        {
            var filter = new Filter { IpoStatuses = new List<IpoStatus> { IpoStatus.Accepted } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnStatusPlanned()
        {
            var filter = new Filter { IpoStatuses = new List<IpoStatus> { IpoStatus.Planned } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
                var data = result.Data.Invitations.First();
                Assert.IsTrue(data.Participants.Count > 1);
                Assert.AreEqual(0, data.Participants.Where(p => p.SignedBy != null).ToList().Count);
                Assert.AreEqual(0, data.Participants.Where(p => p.SignedAtUtc != null).ToList().Count);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutDate_FromNow()
        {
            var filter = new Filter { PunchOutDateFromUtc = _timeProvider.UtcNow };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutDate_FromTwoWeeksInFuture()
        {
            var filter = new Filter { PunchOutDateFromUtc = _timeProvider.UtcNow.AddWeeks(2) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPersonOid()
        {
            var filter = new Filter { PersonOid = _frPersonGuid2 };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnFunctionalRoleCode()
        {
            var filter = new Filter { FunctionalRoleCode = _functionalRoleCode2 };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnCommPkgNoStartWith_ShouldGetOneInvitationWithMcPkgWithCommPkgParent()
        {
            var filter = new Filter { CommPkgNoStartsWith = _commPkgNo2 };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnCommPkgNoStartWith_ShouldGetInvitaionsWithMcPkgAndInvitationWithCommpkg()
        {
            var filter = new Filter { CommPkgNoStartsWith = _commPkgNo };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnMcPkgNoStartWith()
        {
            var filter = new Filter { McPkgNoStartsWith = _mcPkgNo };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutOverdue_NoInvitations()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.Overdue } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutOverdue_TwoInvitations()
        {
            _timeProvider.ElapseWeeks(4);
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.Overdue } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutInNextWeek_OneInvitation()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.NextWeek } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutInNextWeek_ElapseTime_NoInvitations()
        {
            _timeProvider.ElapseWeeks(-1);
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.NextWeek } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutInThisWeek_NoInvitation()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.ThisWeek } };
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnPunchOutInThisWeek_ElapseWeek_OneInvitation()
        {
            _timeProvider.ElapseWeeks(1);
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.ThisWeek } };
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnLastChangedAtFromNow_TwoInvitations()
        {
            var filter = new Filter { LastChangedAtFromUtc = _timeProvider.UtcNow };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnLastChangedAtFromThreeWeeks_NoInvitations()
        {
            var filter = new Filter { LastChangedAtFromUtc = _timeProvider.UtcNow.AddWeeks(3) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnLastChangedAtToLastWeek_NoInvitations()
        {
            var filter = new Filter { LastChangedAtToUtc = _timeProvider.UtcNow.AddWeeks(-1) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldFilterOnLastChangedAtToThreeWeeks_TwoInvitations()
        {
            var filter = new Filter { LastChangedAtToUtc = _timeProvider.UtcNow.AddWeeks(3) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, null, filter);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldSortOnContractorRep_Asc()
        {
            var sorting = new Sorting(SortingDirection.Asc, SortingProperty.ContractorRep);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, sorting);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldSortOnContractorRep_Desc()
        {
            var sorting = new Sorting(SortingDirection.Desc, SortingProperty.ContractorRep);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, sorting);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldSortOnType_Asc()
        {
            var sorting = new Sorting(SortingDirection.Asc, SortingProperty.Type);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, sorting);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldSortOnType_Desc()
        {
            var sorting = new Sorting(SortingDirection.Desc, SortingProperty.Type);

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2, sorting);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);
                Assert.AreEqual(_invitation2.Id, result.Data.Invitations.First().Id);
                Assert.AreEqual(_invitation3.Id, result.Data.Invitations.Last().Id);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_MultipleInvitations_ShouldReturnEmptyHistory()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName2);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);

                AssertCount(result.Data, 2);
                Assert.IsTrue(result.Data.Invitations.All(inv => inv.History.Count == 0));
            }
        }

        [TestMethod]
        public async Task HandleGetInvitationsForExportQuery_ShouldReturnCorrectDto()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsForExportQuery(_projectName);
                var dut = new GetInvitationsForExportQueryHandler(context, _plantProvider, _currentUserProvider, _permissionCache, _exportIpoRepository, _logger);

                var result = await dut.Handle(query, default);

                var invitationDto = result.Data.Invitations.First();
                var invitation = _invitation1;
                AssertInvitation(invitation, invitationDto);
                AssertUsedFilter(result.Data.UsedFilter);
                Assert.AreEqual(4, invitationDto.Participants.Count);
                Assert.AreEqual(1, invitationDto.History.Count);
            }
        }

        private void AssertUsedFilter(UsedFilterDto usedFilter)
        {
            Assert.AreEqual(TestPlant, usedFilter.Plant);
            Assert.AreEqual(_projectName, usedFilter.ProjectName);
            Assert.IsNull(usedFilter.IpoIdStartsWith);
            Assert.IsNull(usedFilter.IpoTitleStartWith);
            Assert.IsNull(usedFilter.McPkgNoStartsWith);
            Assert.IsNull(usedFilter.CommPkgNoStartWith);
            Assert.AreEqual(0, usedFilter.IpoStatuses.Count());
            Assert.IsNull(usedFilter.PunchOutDateFromUtc);
            Assert.IsNull(usedFilter.PunchOutDateToUtc);
            Assert.IsNull(usedFilter.LastChangedFromUtc);
            Assert.IsNull(usedFilter.LastChangedToUtc);
            Assert.IsNull(usedFilter.FunctionalRoleInvited);
            Assert.IsNull(usedFilter.PersonInvited);
        }

        private void AssertInvitation(Invitation invitation, ExportInvitationDto invitationDto)
        {
            Assert.AreEqual(0, invitationDto.CommPkgs.Count());
            Assert.AreEqual(GetProjectById(invitation.ProjectId).Name, invitationDto.ProjectName);
            Assert.AreEqual(invitation.Title, invitationDto.Title);
            Assert.AreEqual(invitation.Description, invitationDto.Description);
            Assert.AreEqual(invitation.Id, invitationDto.Id);
            Assert.AreEqual(invitation.Location, invitationDto.Location);
            Assert.AreEqual(invitation.Type.ToString(), invitationDto.Type);
            Assert.AreEqual(
                invitation.Participants.Single(p => p.SortKey == 0 && p.Type == IpoParticipantType.FunctionalRole)
                    .FunctionalRoleCode, invitationDto.ContractorRep);
            Assert.IsTrue(invitationDto.ConstructionCompanyRep.StartsWith(invitation.Participants.Single(p => p.SortKey == 1).FirstName));
            Assert.IsTrue(invitationDto.Participants.First(p => p.Organization == "Supplier").Participant
                .StartsWith(invitation.Participants.First(p => p.Organization == Organization.Supplier).FirstName));
            Assert.AreEqual(invitation.McPkgs.Single().McPkgNo, invitationDto.McPkgs.Single());
            Assert.AreEqual(invitation.Status, invitationDto.Status);
            Assert.AreEqual(invitation.EndTimeUtc, invitationDto.EndTimeUtc);
            Assert.AreEqual(invitation.StartTimeUtc, invitationDto.StartTimeUtc);
            Assert.AreEqual(invitation.CompletedAtUtc, invitationDto.CompletedAtUtc);
            Assert.AreEqual(invitation.AcceptedAtUtc, invitationDto.AcceptedAtUtc);
            Assert.IsNotNull(invitationDto.CreatedBy);
        }

        private void AssertCount(ExportDto data, int count) 
            => Assert.AreEqual(count, data.Invitations.Count);
    }
}
