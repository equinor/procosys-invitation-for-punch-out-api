using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs;
using Equinor.ProCoSys.IPO.Query.GetOutstandingIpos;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetOutstandingIpos
{
    [TestClass]
    public class GetOutstandingIposForCurrentPersonQueryHandlerTestsBase : ReadOnlyTestsBaseSqlLiteInMemory
    {
        protected Mock<IMeApiService> _meApiServiceMock;
        protected Mock<ICurrentUserProvider> _currentUserProviderMock;
        protected Mock<ILogger<GetOutstandingIposForCurrentPersonQueryHandler>> _loggerMock;
        protected GetOutstandingIposForCurrentPersonQuery _query;
        protected Person _person;
        protected Participant _personParticipantContractor;
        protected Participant _personParticipant2;
        protected Participant _functionalRoleParticipantConstructionCompany;
        protected Participant _personParticipantConstructionCompany;
        protected Participant _personParticipantSupplier;
        protected Participant _personParticipantOperation;
        protected Participant _functionalRoleParticipantContractor;
        protected Participant _personParticipantClosedProject;
        protected Participant _personParticipantNonClosedProject;

        protected Invitation _acceptedInvitationWithOperationPerson;
        protected Invitation _invitationWithPersonParticipantContractor;
        protected Invitation _invitationWithFunctionalRoleParticipantConstructionCompany;
        protected Invitation _cancelledInvitation;
        protected Invitation _invitationWithPersonParticipantConstructionCompany;
        protected Invitation _invitationWithFunctionalRoleParticipantContractor;
        protected Invitation _invitationForClosedProject;
        protected Invitation _invitationForNotClosedProject;
        protected string _functionalRoleCode = "FR1";
        protected const string _closedProjectInvitationTitle = "InvitationTitleForClosedProject";
        protected const string _notClosedProjectInvitationTitle = "InvitationTitleForNOTClosedProject";
        protected const string _closedProjectInvitationDescription = "InvitationDescriptionForClosedProject";
        protected const string _notClosedProjectInvitationDescription = "InvitationDescriptionForNOTClosedProject";
        protected Project _testProject;
        protected Project _testProjectClosed;



        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            _loggerMock = new Mock<ILogger<GetOutstandingIposForCurrentPersonQueryHandler>>();

            using var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

            _query = new GetOutstandingIposForCurrentPersonQuery();
            _person = context.Persons.FirstOrDefault();
            //_person = new Person(_currentUserOid, "test@email.com", "FirstName", "LastName", "UserName");
            //_person.SetProtectedIdForTesting(2);
           
            //context.Persons.Add(_person);

            //context.SaveChangesAsync().Wait();

            _testProject = new Project(TestPlant, "TestProject", "Description for TestProject");
            _testProject.SetProtectedIdForTesting(1);

            _testProjectClosed = new Project(TestPlant, "TestProject", "Description for TestProject") { IsClosed = true };
            _testProjectClosed.SetProtectedIdForTesting(2);

            context.Projects.Add(_testProject);
            context.Projects.Add(_testProjectClosed);

            IList<string> pcsFunctionalRoleCodes = new List<string> { _functionalRoleCode };

            _meApiServiceMock = new Mock<IMeApiService>();
            _meApiServiceMock
                .Setup(x => x.GetFunctionalRoleCodesAsync(TestPlant))
                .Returns(Task.FromResult(pcsFunctionalRoleCodes));

            var helperPerson = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                0);


            SetupInvitationContractor(context);

            SetupInvitationFunctionalRoleConstructionCompany(context, helperPerson);

            SetupInvitationCancelled(context);

            SetupInvitationWithParticipantConstructionCompany(context, helperPerson);
            SetupInvitationFunctionalRoleContractor(context);

            SetupInvitationWithOperationPerson(context, helperPerson);

            SetupInvitationForClosedProject(context);

            SetupInvitationForNotClosedProject(context);

            SetRequiredProperties(context,
                _invitationWithPersonParticipantContractor,
                _invitationWithFunctionalRoleParticipantConstructionCompany,
                _cancelledInvitation,
                _invitationWithPersonParticipantConstructionCompany,
                _invitationWithFunctionalRoleParticipantContractor,
                _acceptedInvitationWithOperationPerson,
                _invitationForClosedProject,
                _invitationForNotClosedProject
            );

            context.SaveChangesAsync().Wait();
        }

        private void SetupInvitationContractor(IPOContext context)
        {
            _invitationWithPersonParticipantContractor = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation1",
                "TestDescription1",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                null);

            _invitationWithPersonParticipantContractor.SetCreated(_person);
            context.Invitations.Add(_invitationWithPersonParticipantContractor);

            _personParticipantContractor = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                0);

            _personParticipantContractor.SetProtectedIdForTesting(2);

            _invitationWithPersonParticipantContractor.AddParticipant(_personParticipantContractor);

            _personParticipantSupplier = new Participant(
                TestPlant,
                Organization.Supplier,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                6);
            _personParticipantSupplier.SetProtectedIdForTesting(6);

            _invitationWithPersonParticipantContractor.AddParticipant(_personParticipantSupplier);
        }

        private void SetupInvitationFunctionalRoleConstructionCompany(IPOContext context, Participant helperPerson)
        {
            _invitationWithFunctionalRoleParticipantConstructionCompany = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation2",
                "TestDescription2",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                null);

            context.Invitations.Add(_invitationWithFunctionalRoleParticipantConstructionCompany);

            _invitationWithFunctionalRoleParticipantConstructionCompany.CompleteIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());

            _functionalRoleParticipantConstructionCompany = new Participant(
                TestPlant,
                Organization.ConstructionCompany,
                IpoParticipantType.FunctionalRole,
                _functionalRoleCode,
                null,
                null,
                null,
                null,
                null,
                1);

            _functionalRoleParticipantConstructionCompany.SetProtectedIdForTesting(1);

            _invitationWithFunctionalRoleParticipantConstructionCompany.SetCreated(_person);
            _invitationWithFunctionalRoleParticipantConstructionCompany.AddParticipant(_functionalRoleParticipantConstructionCompany);
        }

        private void SetupInvitationCancelled(IPOContext context)
        {
            _cancelledInvitation = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation3",
                "TestDescription3",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                null);

            context.Invitations.Add(_cancelledInvitation);

            _cancelledInvitation.CancelIpo(_person);

            _personParticipant2 = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                3);
            _personParticipant2.SetProtectedIdForTesting(3);

            _cancelledInvitation.AddParticipant(_personParticipant2);
        }

        private void SetupInvitationFunctionalRoleContractor(IPOContext context)
        {
            _invitationWithFunctionalRoleParticipantContractor = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation5",
                "TestDescription5",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                null);

            context.Invitations.Add(_invitationWithFunctionalRoleParticipantContractor);

            _functionalRoleParticipantContractor = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                _functionalRoleCode,
                null,
                null,
                null,
                null,
                null,
                0);
            _functionalRoleParticipantContractor.SetProtectedIdForTesting(4);

            _invitationWithFunctionalRoleParticipantContractor.AddParticipant(_functionalRoleParticipantContractor);
        }

        private void SetupInvitationWithParticipantConstructionCompany(IPOContext context, Participant helperPerson)
        {
            _invitationWithPersonParticipantConstructionCompany = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation4",
                "TestDescription4",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                null);

            context.Invitations.Add(_invitationWithPersonParticipantConstructionCompany);

            _personParticipantConstructionCompany = new Participant(
                TestPlant,
                Organization.ConstructionCompany,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                1);

            _personParticipantConstructionCompany.SetProtectedIdForTesting(5);

            _invitationWithPersonParticipantConstructionCompany.AddParticipant(_personParticipantConstructionCompany);

            _invitationWithPersonParticipantConstructionCompany.CompleteIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());
        }

        private void SetupInvitationWithOperationPerson(IPOContext context, Participant helperPerson)
        {
            _acceptedInvitationWithOperationPerson = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation6",
                "TestDescription6",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                null);

            context.Invitations.Add(_acceptedInvitationWithOperationPerson);

            _acceptedInvitationWithOperationPerson.CompleteIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());
            _acceptedInvitationWithOperationPerson.AcceptIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());

            _personParticipantOperation = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                7);
            _personParticipantOperation.SetProtectedIdForTesting(7);

            _acceptedInvitationWithOperationPerson.AddParticipant(_personParticipantOperation);
        }

        private void SetupInvitationForClosedProject(IPOContext context)
        {
            _invitationForClosedProject = new Invitation(
                TestPlant,
                _testProjectClosed,
                _closedProjectInvitationTitle,
                _closedProjectInvitationDescription,
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProjectClosed, "Comm", "Mc", "d", "1|2") },
                null);

            context.Invitations.Add(_invitationForClosedProject);

            _personParticipantClosedProject = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                7);
            _personParticipantClosedProject.SetProtectedIdForTesting(8);

            _invitationForClosedProject.AddParticipant(_personParticipantClosedProject);
        }

        private void SetupInvitationForNotClosedProject(IPOContext context)
        {
            _invitationForNotClosedProject = new Invitation(
                TestPlant,
                _testProject,
                _notClosedProjectInvitationTitle,
                _notClosedProjectInvitationDescription,
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                null);

            context.Invitations.Add(_invitationForNotClosedProject);

            _personParticipantNonClosedProject = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                7);

            _personParticipantNonClosedProject.SetProtectedIdForTesting(9);

            _invitationForNotClosedProject.AddParticipant(_personParticipantNonClosedProject);
        }

        private void SetRequiredProperties(IPOContext ipoContext, params Invitation[] invitations)
        {
            foreach (var invitation in invitations)
            {
                var comment = new Comment(TestPlant, "c");
                comment.SetCreated(_person);
                ipoContext.Comments.Add(comment);
                invitation.AddComment(comment);

                var attachment = new Attachment(TestPlant, "f.txt");
                attachment.SetCreated(_person);
                ipoContext.Attachments.Add(attachment);
                invitation.AddAttachment(attachment);

                invitation.SetCreated(_person);
                invitation.SetRowVersion("abc");
            }
        }
    }
}
