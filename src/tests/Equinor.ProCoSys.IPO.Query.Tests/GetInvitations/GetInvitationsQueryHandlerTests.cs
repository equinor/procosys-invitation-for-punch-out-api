﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetInvitations;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetInvitations
{
    [TestClass]
    public class GetInvitationsQueryHandlerTests : ReadOnlyTestsBase
    {
        private Invitation _invitation1;
        private Invitation _invitation2;
        private Invitation _invitation3;

        private string _functionalRoleCode1 = "FrCode1";
        private string _functionalRoleCode2 = "FrCode2";
        private readonly Guid _personGuid = new Guid("11111111-2222-2222-2222-333333333333");
        private readonly Guid _frPersonGuid1 = new Guid("11111111-2222-2222-2222-333333333332"); 
        private readonly Guid _frPersonGuid2 = new Guid("11111111-2222-2222-2222-333333333335"); 
        private string _frEmail1 = "FR1@email.com";
        private string _personEmail1 = "P1@email.com";
        private string _personEmail2 = "P2@email.com";
        private string _frPersonEmail1 = "frp1@email.com";
        private string _frPersonEmail2 = "frp2@email.com";
        const string _projectName = "Project1";
        const string _projectName2 = "Project2";
        const string _title1 = "Title1";
        const string _title2 = "Title2";
        const string _commPkgNo = "CommPkgNo";
        const string _commPkgNo2 = "CommPkgNo2";
        const string _mcPkgNo = "McPkgNo";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
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
                    _currentUserOid,
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
                    _currentUserOid,
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

                var commPkg = new CommPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    description,
                    "OK",
                    "1|2");

                var mcPkg1 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo,
                    _mcPkgNo,
                    description);

                var mcPkg2 = new McPkg(
                    TestPlant,
                    _projectName,
                    _commPkgNo2,
                    _mcPkgNo,
                    description);

                _invitation1 = new Invitation(
                    TestPlant,
                    _projectName,
                    _title1,
                    "Description",
                    DisciplineType.DP,
                    new DateTime(2020, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                    new DateTime(2020, 9, 1, 11, 0, 0, DateTimeKind.Utc),
                    null);

                _invitation1.AddParticipant(functionalRoleParticipant1);
                _invitation1.AddParticipant(personParticipant2);
                _invitation1.AddParticipant(frPerson1);
                _invitation1.AddMcPkg(mcPkg1);

                _invitation2 = new Invitation(
                    TestPlant,
                    _projectName2,
                    _title1,
                    "Description",
                    DisciplineType.MDP,
                    new DateTime(2019, 10, 1, 10, 0, 0, DateTimeKind.Utc),
                    new DateTime(2019, 10, 1, 11, 0, 0, DateTimeKind.Utc),
                    null);

                _invitation2.AddParticipant(functionalRoleParticipant2);
                _invitation2.AddParticipant(personParticipant1);
                _invitation2.AddParticipant(frPerson2);
                _invitation2.AddCommPkg(commPkg);

                _invitation3 = new Invitation(
                    TestPlant,
                    _projectName2,
                    _title2,
                    "Description",
                    DisciplineType.DP,
                    new DateTime(2019, 11, 1, 10, 0, 0, DateTimeKind.Utc),
                    new DateTime(2019, 11, 1, 11, 0, 0, DateTimeKind.Utc),
                    null);

                _invitation3.AddParticipant(personParticipant3);
                _invitation3.AddParticipant(personParticipant4);
                _invitation3.AddMcPkg(mcPkg2);

                context.Invitations.Add(_invitation1);
                context.Invitations.Add(_invitation2);
                context.Invitations.Add(_invitation3);
                context.SaveChangesAsync().Wait();
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnOkResult()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName);
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnStatus()
        {
            var filter = new Filter { IpoStatuses = new List<IpoStatus> {IpoStatus.Accepted} };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnStatus2()
        {
            var filter = new Filter { IpoStatuses = new List<IpoStatus> { IpoStatus.Planned } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutDate()
        {
            var filter = new Filter { PunchOutDateFromUtc = new DateTime(2019, 10, 20, 10, 0, 0, DateTimeKind.Utc) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnCommPkgNoStartWith()
        {
            var filter = new Filter { CommPkgNoStartsWith = _commPkgNo2 };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnCommPkgNoStartWith2()
        {
            var filter = new Filter { CommPkgNoStartsWith = _commPkgNo };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

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
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 1);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutOverdue()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.Overdue } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnPunchOutInNextWeek()
        {
            var filter = new Filter { PunchOutDates = new List<PunchOutDateFilterType> { PunchOutDateFilterType.NextWeek } };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data,0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtFrom()
        {
            var filter = new Filter { LastChangedAtFromUtc = new DateTime(2019, 10, 20, 10, 0, 0, DateTimeKind.Utc) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtFrom2()
        {
            var filter = new Filter { LastChangedAtFromUtc = new DateTime(2020, 10, 20, 10, 0, 0, DateTimeKind.Utc) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtTo()
        {
            var filter = new Filter { LastChangedAtToUtc = new DateTime(2019, 10, 20, 10, 0, 0, DateTimeKind.Utc) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 0);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldFilterOnLastChangedAtTo2()
        {
            var filter = new Filter { LastChangedAtToUtc = new DateTime(2020, 10, 20, 10, 0, 0, DateTimeKind.Utc) };

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName2, null, filter);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);
                AssertCount(result.Data, 2);
            }
        }

        [TestMethod]
        public async Task Handler_ShouldReturnCorrectDto()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var query = new GetInvitationsQuery(_projectName);
                var dut = new GetInvitationsQueryHandler(context);

                var result = await dut.Handle(query, default);

                var invitationDto = result.Data.Invitations.First();
                var invitation = _invitation1;
                Assert.AreEqual(0, invitationDto.CommPkgNos.Count);
                Assert.AreEqual(invitation.ProjectName, invitationDto.ProjectName);
                Assert.AreEqual(invitation.Title, invitationDto.Title);
                Assert.AreEqual(invitation.Description, invitationDto.Description);
                Assert.AreEqual(invitation.Id, invitationDto.Id);
                Assert.AreEqual(invitation.Location, invitationDto.Location);
                Assert.AreEqual(invitation.Type, invitationDto.Type);
                Assert.AreEqual(
                    invitation.Participants.Single(p => p.SortKey == 0 && p.Type == IpoParticipantType.FunctionalRole)
                        .FunctionalRoleCode, invitationDto.ContractorRep);
                Assert.IsTrue(invitationDto.ConstructionCompanyRep.StartsWith(invitation.Participants.Single(p => p.SortKey == 1).FirstName));
                Assert.AreEqual(invitation.McPkgs.Single().McPkgNo, invitationDto.McPkgNos.Single());
                Assert.AreEqual(invitation.Status, invitationDto.Status);
                Assert.AreEqual(invitation.EndTimeUtc, invitationDto.EndTimeUtc);
                Assert.AreEqual(invitation.StartTimeUtc, invitationDto.StartTimeUtc);
                Assert.AreEqual(invitation.CompletedAtUtc, invitationDto.CompletedAtUtc);
                Assert.AreEqual(invitation.AcceptedAtUtc, invitationDto.AcceptedAtUtc);
            }
        }

        private void AssertCount(InvitationsResult data, int count)
        {
            Assert.AreEqual(count, data.MaxAvailable);
            Assert.AreEqual(count, data.Invitations.Count());
        }
    }
}
