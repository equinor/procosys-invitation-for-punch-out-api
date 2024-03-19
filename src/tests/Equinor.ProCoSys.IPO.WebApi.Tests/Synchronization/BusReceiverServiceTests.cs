using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Synchronization
{
    [TestClass]
    public class BusReceiverServiceTests
    {
        private Mock<ICurrentUserSetter> _currentUserSetter;
        private BusReceiverService _dut;
        private Mock<IUnitOfWork> _unitOfWork;
        private Mock<IInvitationRepository> _invitationRepository;
        private Mock<IPlantSetter> _plantSetter;
        private Mock<ITelemetryClient> _telemetryClient;
        private Mock<IMcPkgApiService> _mcPkgApiService;
        private Mock<IReadOnlyContext> _readOnlyContext;
        private Mock<IMainApiAuthenticator> _mainApiAuthenticator;
        private Mock<IProjectRepository> _projectRepository;
        private Mock<ICertificateEventProcessorService> _certificationEventProcessorService;

        private const string plant = "PCS$HEIMDAL";
        private static readonly Project project1 = new(plant, project1Name, $"Description of {project1Name} project", _project1Guid);
        private const int project1Id = 320;
        private const string project1Name = "HEIMDAL";
        private const string project2Name = "XYZ";

        private static readonly Guid _project1Guid = new Guid("11111111-2222-2222-2222-333333333341");
        private static readonly Guid _project2Guid = new Guid("11111111-2222-2222-2222-333333333342");

        private const string commPkgNo1 = "123";
        private const string commPkgNo2 = "234";
        private const string commPkgNo3 = "456";
        private const string mcPkgNo1 = "456";
        private const string mcPkgNo3 = "333";
        private const string mcPkgNo4 = "444";
        private const string description = "789";
        private const string functionalRoleCodeOld = "IPO FR1 TEST";
        private const string functionalRoleCodeNew = "IPO FR2 TEST";
        private const string librarytypefunctionalrole = "FUNCTIONAL_ROLE";

        private Guid _mcPkgGuid = new Guid("11111111-2222-2222-3333-333333333333");
        private Guid _commPkgGuid = new Guid("11111111-2222-2222-4444-444444444444");

        private List<McPkg> _mcPkgsOn1 = new List<McPkg>
        {
            new McPkg(plant, project1, commPkgNo2, mcPkgNo1, description, "1|2", Guid.Empty, Guid.Empty)
        };

        private List<CommPkg> _commPkgsOn2 = new List<CommPkg>
        {
            new CommPkg(plant, project1, commPkgNo1, description,"status", "1|2",Guid.Empty),
            new CommPkg(plant, project1, commPkgNo2, description, "status", "1|2", Guid.Empty)
        };

        private List<McPkg> _mcPkgsOn3 = new List<McPkg>
        {
            new McPkg(plant, project1, commPkgNo3, mcPkgNo3, description, "1|2", Guid.Empty, Guid.Empty)
        };

        private List<McPkg> _mcPkgsOn4 = new List<McPkg>
        {
            new McPkg(plant, project1, commPkgNo3, mcPkgNo4, description, "1|2", Guid.Empty, Guid.Empty)
        };

        private Invitation _invitation1, _invitation2, _invitation3, _invitation4;
        private Mock<IOptionsSnapshot<IpoAuthenticatorOptions>> _options;

        [TestInitialize]
        public void Setup()
        {
            project1.SetProtectedIdForTesting(project1Id);

            _invitationRepository = new Mock<IInvitationRepository>();
            _projectRepository = new Mock<IProjectRepository>();
            _plantSetter = new Mock<IPlantSetter>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _telemetryClient = new Mock<ITelemetryClient>();
            _mcPkgApiService = new Mock<IMcPkgApiService>();
            _readOnlyContext = new Mock<IReadOnlyContext>();
            _mainApiAuthenticator = new Mock<IMainApiAuthenticator>();
            _certificationEventProcessorService = new Mock<ICertificateEventProcessorService>();

            _invitation1 = new Invitation(plant, project1, "El invitasjån", description, DisciplineType.DP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån", _mcPkgsOn1, null);
            _invitation2 = new Invitation(plant, project1, "El invitasjån2", description, DisciplineType.MDP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån2", null, _commPkgsOn2);
            _invitation3 = new Invitation(plant, project1, "El invitasjån3", description, DisciplineType.DP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån3", _mcPkgsOn3, null);
            _invitation4 = new Invitation(plant, project1, "El invitasjån4", description, DisciplineType.DP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån4", _mcPkgsOn4, null);

            _options = new Mock<IOptionsSnapshot<IpoAuthenticatorOptions>>();
            _options.Setup(s => s.Value).Returns(new IpoAuthenticatorOptions { IpoApiObjectId = Guid.NewGuid() });
            _currentUserSetter = new Mock<ICurrentUserSetter>();

            _projectRepository.Setup(x => x.GetByIdAsync(project1Id)).Returns(Task.FromResult(project1));

            _dut = new BusReceiverService(_invitationRepository.Object,
                                          _plantSetter.Object,
                                          _unitOfWork.Object,
                                          _telemetryClient.Object,
                                          _readOnlyContext.Object,
                                          _mcPkgApiService.Object,
                                          _mainApiAuthenticator.Object,
                                          _options.Object,
                                          _currentUserSetter.Object,
                                          _projectRepository.Object,
                                          _certificationEventProcessorService.Object);

            var list = new List<Invitation> {_invitation1, _invitation2, _invitation3, _invitation4};
            _readOnlyContext.Setup(r => r.QuerySet<Invitation>()).Returns(list.AsQueryable());
        }

        [TestMethod]

        public async Task HandlingCommPkgTopicWithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopicConstants.CommPkg, message, new CancellationToken(false));

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(project1Name, commPkgNo2, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingCommPkgTopic_Move_WithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project2Name}\", \"ProjectNameOld\" : \"{project1Name}\", \"CommPkgNo\" :\"{commPkgNo3}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopicConstants.CommPkg, message, new CancellationToken(false));

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.MoveCommPkg(project1Name, project2Name, commPkgNo3, description));
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]

        public async Task HandlingCommPkgTopic_ShouldCallMoveCommPkgOnInvitationRepository()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectNameOld\" : \"{project1Name}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"Description\" : \"{description}\"}}";
            
            await _dut.ProcessMessageAsync(PcsTopicConstants.CommPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        public async Task HandlingMcPkgTopicWithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"McPkgNo\" : \"{mcPkgNo1}\", \"Description\" : \"{description}\", \"Milestones\": [{{\"Code\": \"M-01\",\"Planned\": \"2021-12-06T00:00:00.000Z\",\"Actual\": \"12-dec-2021\"}}]}}";
            await _dut.ProcessMessageAsync(PcsTopicConstants.McPkg, message, new CancellationToken(false));

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(project1Name, mcPkgNo1, description, _mcPkgGuid, _commPkgGuid), Times.Once);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingMcPkgTopicWithoutFailure_WhenMoveMcPkg()
        {
            var commPkgOld = "C1";
            var mcPKgOld = "M1";
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"CommPkgNoOld\" :\"{commPkgOld}\", \"McPkgNo\" : \"{mcPkgNo1}\",  \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopicConstants.McPkg, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),It.IsAny<Guid>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingMcPkgTopicShouldFail_WhenMoveMcPkg_MissingCommPkgNoOld()
        {
            var mcPKgOld = "M1";
            var message =
                $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"McPkgNo\" : \"{mcPkgNo1}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopicConstants.McPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingMcPkgTopicShouldFail_WhenMoveMcPkg_MissingMvPkgNoOld()
        {
            var commPkgOld = "C1";
            var message =
                $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"McPkgNo\" : \"{mcPkgNo1}\", \"CommPkgNoOld\" : \"{commPkgOld}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopicConstants.McPkg, message, new CancellationToken(false));
        }

        [TestMethod]

        public async Task HandlingLibraryTopicWithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"Code\" : \"{functionalRoleCodeNew}\", \"CodeOld\" : \"{functionalRoleCodeOld}\", \"Description\" : \"{description}\", \"IsVoided\" : false, \"Type\" : \"{librarytypefunctionalrole}\"}}";

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(It.IsAny<Guid>()), Times.Never);

            await _dut.ProcessMessageAsync(PcsTopicConstants.Library, message, new CancellationToken(false));
            
            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateFunctionalRoleCodesOnInvitations(plant, functionalRoleCodeOld, functionalRoleCodeNew), Times.Once);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingLibraryTopic_ForEventWithoutOldCode_ShouldProcessWithoutFailureAndWithoutUpdatingTheParticipants()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"Code\" : \"{functionalRoleCodeNew}\", \"Description\" : \"{description}\", \"IsVoided\" : false, \"Type\" : \"{librarytypefunctionalrole}\"}}";

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(It.IsAny<Guid>()), Times.Never);

            await _dut.ProcessMessageAsync(PcsTopicConstants.Library, message, new CancellationToken(false));

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Never);
            _invitationRepository.Verify(i => i.UpdateFunctionalRoleCodesOnInvitations(plant, functionalRoleCodeOld, functionalRoleCodeNew), Times.Never);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingLibraryTopic_ForOtherThanFunctionRole_ShouldProcessWithoutFailureAndWithoutUpdatingTheParticipants()
        {
            var otherlibrarytype = "X";
            var message = $"{{\"Plant\" : \"{plant}\", \"Code\" : \"{functionalRoleCodeNew}\", \"Description\" : \"{description}\", \"IsVoided\" : false, \"Type\" : \"{otherlibrarytype}\"}}";

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(It.IsAny<Guid>()), Times.Never);

            await _dut.ProcessMessageAsync(PcsTopicConstants.Library, message, new CancellationToken(false));

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Never);
            _invitationRepository.Verify(i => i.UpdateFunctionalRoleCodesOnInvitations(plant, functionalRoleCodeOld, functionalRoleCodeNew), Times.Never);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingProjectTopicWithoutFailure()
        {
            var projectRepositoryTestDouble = new ProjectRepositoryTestDouble();
            projectRepositoryTestDouble.Add(new Project(plant, project1Name, "Description to be changed", _project1Guid) { IsClosed = false });

            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"IsClosed\" : true, \"Description\" : \"{description}\"}}";

            var preActualProject = await projectRepositoryTestDouble.GetProjectOnlyByNameAsync(project1Name);
            Assert.AreEqual("Description to be changed", preActualProject.Description);
            Assert.IsFalse(preActualProject.IsClosed);

            var dut = new BusReceiverService(_invitationRepository.Object,
                _plantSetter.Object,
                _unitOfWork.Object,
                _telemetryClient.Object,
                _readOnlyContext.Object,
                _mcPkgApiService.Object,
                _mainApiAuthenticator.Object,
                _options.Object,
                _currentUserSetter.Object,
                projectRepositoryTestDouble,
                _certificationEventProcessorService.Object);

            await dut.ProcessMessageAsync(PcsTopicConstants.Project, message, new CancellationToken(false));

            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(project1Name, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            
            var actualProject = await projectRepositoryTestDouble.GetProjectOnlyByNameAsync(project1Name);
            Assert.AreEqual(description, actualProject.Description);
            Assert.IsTrue(actualProject.IsClosed);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForCancelIpo_WithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Canceled";
            
            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.Guid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Ipo, message, new CancellationToken(false));
            
            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> {mcPkgNo1}, new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnCompletedIpo_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "UnCompleted";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.Guid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, null, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once, "Uncompleting an IPO should not sent InvitationId as we want to clear external reference.");
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, It.IsAny<int>(), project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, It.IsAny<int>(), project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, It.IsAny<int>(), project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnknownIpoEvent_ShouldProcessWithoutFailureAndWithoutUpdatingTheIpo()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Canceled2";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.Guid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string>(), new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForCompletedIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Completed";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.Guid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Ipo, message, new CancellationToken(false));

            // Assert
            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForAcceptIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Accepted";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.Guid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Ipo, message, new CancellationToken(false));

            // Assert
            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnAcceptIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "UnAccepted";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.Guid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Ipo, message, new CancellationToken(false));

            // Assert
            _currentUserSetter.Verify(c => c.SetCurrentUserOid(_options.Object.Value.IpoApiObjectId), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1Name, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingCommPkgTopic_ShouldFailIfEmpty()
        {
            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.CommPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingMcPkgTopic_ShouldFailIfEmpty()
        {
            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.McPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingIpoTopic_ShouldFailIfEmpty()
        {
            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Ipo, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingProjectTopic_ShouldFailIfEmpty()
        {
            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Project, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingLibraryTopic_ShouldFailIfEmpty()
        {
            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Library, message, new CancellationToken(false));
        }

       
        [TestMethod]
        public async Task HandleDeleteTopic_ShouldIgnoreMessage()
        {
            // Arrange
            const string Delete = "delete";
            var guid = new Guid();
            var message =
                $"{{\"Plant\" : \"SomePlant\",\"ProCoSysGuid\" : \"{guid}\",\"TagNo\" : \"someTagNo\",\"Behavior\" : \"{Delete}\",\"RegisterCode\" : \"SomeRegister\"}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Tag, message, new CancellationToken(false));

            // Assert
            _telemetryClient.Verify(tc => tc.TrackEvent("IPO Bus Receiver",
                new Dictionary<string, string>
                {
                    {"Event Delete", PcsTopicConstants.Tag.ToString()},
                    {"ProCoSysGuid", guid.ToString()}
                }, null), Times.Once());

            //ProcessMessageAsync should return before setting user
            _currentUserSetter.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task HandleCertificateTopic_ShouldCall_CertificateEventProcessorService()
        {
            // Arrange
            var message =
                $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CertificateNo\" :\"XX\"}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Certificate, message, default);

            // Assert
            _certificationEventProcessorService.Verify(u => u.ProcessCertificateEventAsync(message), Times.Once);
        }

        [TestMethod]
        public async Task HandleCertificateTopic_ShouldNotCallCertificateEventProcessorService_WhenMessageHasDeleteBehavior()
        {
            // Arrange
            var message =
                $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CertificateNo\" :\"XX\", \"Behavior\" :\"delete\"}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Certificate, message, default);

            // Assert
            _certificationEventProcessorService.Verify(u => u.ProcessCertificateEventAsync(message), Times.Never);
        }


        [TestMethod]
        public async Task HandleCertificateTopic_ShouldNotFail_WhenReceivingTheSameMessageTwice()
        {
            // Arrange
            var message =
                $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1Name}\", \"CertificateNo\" :\"XX\"}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopicConstants.Certificate, message, default);
            await _dut.ProcessMessageAsync(PcsTopicConstants.Certificate, message, default);

            // Assert
            _certificationEventProcessorService.Verify(u => u.ProcessCertificateEventAsync(message), Times.Exactly(2));
        }
    }
}
