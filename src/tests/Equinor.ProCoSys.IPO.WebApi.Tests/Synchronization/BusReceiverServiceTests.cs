using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BusReceiver;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
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

        private const string plant = "PCS$HEIMDAL";
        private const string project = "HEIMDAL";
        private const string commPkgNo = "123";
        private const string mcPkgNo = "456";
        private const string description = "789";

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

            _dut = new BusReceiverService(_invitationRepository.Object, _plantSetter.Object, _unitOfWork.Object, _telemetryClient.Object, _readOnlyContext.Object, _fusionMeetingClient.Object, _mcPkgApiService.Object);
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
        public void HandlingIpoTopicWithoutFailure()
        {
            Assert.Fail("Not implemented yet");
        }
    }
}
