using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BusReceiver;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.IPO.WebApi.Telemetry;
using Fusion.Integration.Meeting;
using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Synchronization
{
    [TestClass]
    public class BusReceiverServiceTests
    {
        private BusReceiverService _dut;
        private Mock<IUnitOfWork> _unitOfWork;
        private Mock<IInvitationRepository> _invitationRepository;
        private Mock<IPlantSetter> _plantSetter;
        private Mock<ITelemetryClient> _telemetryClient;
        private Mock<IFusionMeetingClient> _fusionMeetingClient;
        private Mock<IMcPkgApiService> _mcPkgApiService;
        private Mock<IReadOnlyContext> _readOnlyContext;
        private Mock<IApplicationAuthenticator> _applicationAuthenticator;
        private Mock<IBearerTokenSetter> _bearerTokenSetter;

        private const string plant = "PCS$HEIMDAL";
        private const string project = "HEIMDAL";
        private const string commPkgNo = "123";
        private const string mcPkgNo = "456";
        private const string description = "789";

        private Invitation _invitation;

        [TestInitialize]
        public void Setup()
        {
            _invitationRepository = new Mock<IInvitationRepository>();
            _plantSetter = new Mock<IPlantSetter>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _telemetryClient = new Mock<ITelemetryClient>();
            _fusionMeetingClient = new Mock<IFusionMeetingClient>();
            _mcPkgApiService = new Mock<IMcPkgApiService>();
            _readOnlyContext = new Mock<IReadOnlyContext>();
            _applicationAuthenticator = new Mock<IApplicationAuthenticator>();
            _bearerTokenSetter = new Mock<IBearerTokenSetter>();

            _invitation = new Invitation(plant, project, "El invitasjån", description, DisciplineType.DP, DateTime.Now, DateTime.Now.AddHours(1), "El låkasjån" );
            _dut = new BusReceiverService(_invitationRepository.Object,
                                          _plantSetter.Object,
                                          _unitOfWork.Object,
                                          _telemetryClient.Object,
                                          _readOnlyContext.Object,
                                          _fusionMeetingClient.Object,
                                          _mcPkgApiService.Object,
                                          _applicationAuthenticator.Object,
                                          _bearerTokenSetter.Object);

            var list = new List<Invitation> {_invitation};
            _readOnlyContext.Setup(r => r.QuerySet<Invitation>()).Returns(list.AsQueryable());
        }

        [TestMethod]
        public async Task HandlingCommPkgTopicWithoutFailure()
        {
            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"ProjectName\" : \"{project}\", \"CommPkgNo\" :\"{commPkgNo}\", \"Description\" : \"{description}\"}}"));
            await _dut.ProcessMessageAsync(PcsTopic.CommPkg, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(project, commPkgNo, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingMcPkgTopicWithoutFailure()
        {
            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"ProjectName\" : \"{project}\", \"McPkgNo\" : \"{mcPkgNo}\", \"Description\" : \"{description}\"}}"));
            await _dut.ProcessMessageAsync(PcsTopic.McPkg, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(project, mcPkgNo, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingProjectTopicWithoutFailure()
        {
            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"ProjectName\" : \"{project}\", \"Description\" : \"{description}\"}}"));
            await _dut.ProcessMessageAsync(PcsTopic.Project, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(project, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForCancelIpo_WithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Canceled";
            
            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}"));

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));
            
            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _fusionMeetingClient.Verify(f => f.DeleteMeetingAsync(_invitation.MeetingId));
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnknownIpoEvent_ShouldProcessWithoutFailureAndWithoutUpdatingTheIpo()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Canceled2";

            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}"));

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _fusionMeetingClient.Verify(f => f.DeleteMeetingAsync(_invitation.MeetingId), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForCompletedIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Completed";

            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}"));

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _fusionMeetingClient.Verify(f => f.DeleteMeetingAsync(_invitation.MeetingId), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForAcceptIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Accepted";

            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}"));

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Once);
            _fusionMeetingClient.Verify(f => f.DeleteMeetingAsync(_invitation.MeetingId), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnAcceptIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "UnAccepted";

            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}"));

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation.Id, project, new List<string>(), new List<string>()), Times.Never);
            _fusionMeetingClient.Verify(f => f.DeleteMeetingAsync(_invitation.MeetingId), Times.Never);
        }
    }
}
