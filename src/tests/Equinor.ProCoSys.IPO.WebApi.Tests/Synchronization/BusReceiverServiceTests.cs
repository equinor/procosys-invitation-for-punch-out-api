using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BusReceiver;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
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
            _dut = new BusReceiverService(_invitationRepository.Object, _plantSetter.Object, _unitOfWork.Object);
        }

        [TestMethod]
        public async Task HandlingCommPkgTopicWithoutFailure()
        {
            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"ProjectName\" : \"{project}\", \"CommPkgNo\" :\"{commPkgNo}\", \"Description\" : \"{description}\"}}"));
            await _dut.ProcessMessageAsync(PcsTopic.Commpkg, message, new CancellationToken(false));

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
            await _dut.ProcessMessageAsync(PcsTopic.Mcpkg, message, new CancellationToken(false));

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
        public async Task HandlingIncorrectMessageJsonShouldThrowException()
        {
            var message = new Message(Encoding.UTF8.GetBytes($"{{\"ProjectSchema\" : \"{plant}\", \"ProjectNadme\" : \"{project}\", \"Description\" : \"{description}\"}}"));
            await _dut.ProcessMessageAsync(PcsTopic.Project, message, new CancellationToken(false));

            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(null, description), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
