using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.IPO.WebApi.Telemetry;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.ProCoSys.PcsServiceBus.Topics;
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
        private Mock<IMcPkgApiService> _mcPkgApiService;
        private Mock<IReadOnlyContext> _readOnlyContext;
        private Mock<IApplicationAuthenticator> _applicationAuthenticator;
        private Mock<IBearerTokenSetter> _bearerTokenSetter;

        private const string plant = "PCS$HEIMDAL";
        private const string project1 = "HEIMDAL";
        private const string project2 = "XYZ";
        private const string commPkgNo1 = "123";
        private const string commPkgNo2 = "234";
        private const string commPkgNo3 = "456";
        private const string mcPkgNo1 = "456";
        private const string mcPkgNo2 = "567";
        private const string mcPkgNo3 = "333";
        private const string mcPkgNo4 = "444";
        private const string description = "789";

        private List<McPkg> _mcPkgsOn1 = new List<McPkg>
        {
            new McPkg(plant, project1, commPkgNo2, mcPkgNo1, description, "1|2")
        };

        private List<CommPkg> _commPkgsOn2 = new List<CommPkg>
        {
            new CommPkg(plant, project1, commPkgNo1, description,"status", "1|2"),
            new CommPkg(plant, project1, commPkgNo2, description, "status", "1|2")
        };

        private List<McPkg> _mcPkgsOn3 = new List<McPkg>
        {
            new McPkg(plant, project1, commPkgNo3, mcPkgNo3, description, "1|2")
        };

        private List<McPkg> _mcPkgsOn4 = new List<McPkg>
        {
            new McPkg(plant, project1, commPkgNo3, mcPkgNo4, description, "1|2")
        };

        private Invitation _invitation1, _invitation2, _invitation3, _invitation4;

        [TestInitialize]
        public void Setup()
        {
            _invitationRepository = new Mock<IInvitationRepository>();
            _plantSetter = new Mock<IPlantSetter>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _telemetryClient = new Mock<ITelemetryClient>();
            _mcPkgApiService = new Mock<IMcPkgApiService>();
            _readOnlyContext = new Mock<IReadOnlyContext>();
            _applicationAuthenticator = new Mock<IApplicationAuthenticator>();
            _bearerTokenSetter = new Mock<IBearerTokenSetter>();
            _invitation1 = new Invitation(plant, project1, "El invitasjån", description, DisciplineType.DP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån", _mcPkgsOn1, null);
            _invitation2 = new Invitation(plant, project1, "El invitasjån2", description, DisciplineType.MDP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån2", null, _commPkgsOn2);
            _invitation3 = new Invitation(plant, project1, "El invitasjån3", description, DisciplineType.DP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån3", _mcPkgsOn3, null);
            _invitation4 = new Invitation(plant, project1, "El invitasjån4", description, DisciplineType.DP, DateTime.Now,
                DateTime.Now.AddHours(1), "El låkasjån4", _mcPkgsOn4, null);

            _dut = new BusReceiverService(_invitationRepository.Object,
                                          _plantSetter.Object,
                                          _unitOfWork.Object,
                                          _telemetryClient.Object,
                                          _readOnlyContext.Object,
                                          _mcPkgApiService.Object,
                                          _applicationAuthenticator.Object,
                                          _bearerTokenSetter.Object);

            var list = new List<Invitation> {_invitation1, _invitation2, _invitation3, _invitation4};
            _readOnlyContext.Setup(r => r.QuerySet<Invitation>()).Returns(list.AsQueryable());
        }

        [TestMethod]

        public async Task HandlingCommPkgTopicWithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopic.CommPkg, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(project1, commPkgNo2, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingCommPkgTopic_Move_WithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project2}\", \"ProjectNameOld\" : \"{project1}\", \"CommPkgNo\" :\"{commPkgNo3}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopic.CommPkg, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.MoveCommPkg(project1, project2, commPkgNo3, description));
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]

        public async Task HandlingCommPkgTopic_ShouldCallMoveCommPkgOnInvitationRepository()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectNameOld\" : \"{project1}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"Description\" : \"{description}\"}}";
            
            await _dut.ProcessMessageAsync(PcsTopic.CommPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        public async Task HandlingMcPkgTopicWithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"McPkgNo\" : \"{mcPkgNo1}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopic.McPkg, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(project1, mcPkgNo1, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingMcPkgTopicWithoutFailure_WhenMoveMcPkg()
        {
            var commPkgOld = "C1";
            var mcPKgOld = "M1";
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"CommPkgNoOld\" :\"{commPkgOld}\", \"McPkgNo\" : \"{mcPkgNo1}\", \"McPkgNoOld\" : \"{mcPKgOld}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopic.McPkg, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.MoveMcPkg(project1, commPkgOld, commPkgNo2, mcPKgOld, mcPkgNo1, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingMcPkgTopicShouldFail_WhenMoveMcPkg_MissingCommPkgNoOld()
        {
            var mcPKgOld = "M1";
            var message =
                $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"McPkgNo\" : \"{mcPkgNo1}\", \"McPkgNoOld\" : \"{mcPKgOld}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopic.McPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingMcPkgTopicShouldFail_WhenMoveMcPkg_MissingMvPkgNoOld()
        {
            var commPkgOld = "C1";
            var message =
                $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1}\", \"CommPkgNo\" :\"{commPkgNo2}\", \"McPkgNo\" : \"{mcPkgNo1}\", \"CommPkgNoOld\" : \"{commPkgOld}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopic.McPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        public void Testlkj()
        {
            var k = "{ \"Plant\" : \"PCS$HEIDRUN\", \"ProjectName\" : \"M.O095C.20.A.0014\", \"CommPkgNo\" : \"7303-C01\", \"McPkgNo\" : \"7303-M001\", \"Description\" : \"Midlertidig kran\"}";
            var mcPkgTopic = JsonSerializer.Deserialize<McPkgTopic>(k);
        }
        

    [TestMethod]
        public async Task HandlingProjectTopicWithoutFailure()
        {
            var message = $"{{\"Plant\" : \"{plant}\", \"ProjectName\" : \"{project1}\", \"Description\" : \"{description}\"}}";
            await _dut.ProcessMessageAsync(PcsTopic.Project, message, new CancellationToken(false));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _plantSetter.Verify(p => p.SetPlant(plant), Times.Once);
            _invitationRepository.Verify(i => i.UpdateProjectOnInvitations(project1, description), Times.Once);
            _invitationRepository.Verify(i => i.UpdateMcPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _invitationRepository.Verify(i => i.UpdateCommPkgOnInvitations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForCancelIpo_WithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Canceled";
            
            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));
            
            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1, new List<string> {mcPkgNo1}, new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnCompletedIpo_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "UnCompleted";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, null, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once, "Uncompleting an IPO should not sent InvitationId as we want to clear external reference.");
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, It.IsAny<int>(), project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, It.IsAny<int>(), project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, It.IsAny<int>(), project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnknownIpoEvent_ShouldProcessWithoutFailureAndWithoutUpdatingTheIpo()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Canceled2";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1, new List<string>(), new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1, new List<string>(), new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForCompletedIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Completed";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForAcceptIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "Accepted";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingIpoTopic_ForUnAcceptIpoEvent_ShouldProcessWithoutFailure()
        {
            // Arrange
            var status = 1;
            var ipoEvent = "UnAccepted";

            var message = $"{{\"Plant\" : \"{plant}\", \"InvitationGuid\" : \"{_invitation1.ObjectGuid}\", \"Event\" : \"{ipoEvent}\", \"Status\" : {status}}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));

            // Assert
            _plantSetter.Verify(p => p.SetPlant(plant));
            _mcPkgApiService.Verify(m => m.ClearM01DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.SetM01DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
            _mcPkgApiService.Verify(m => m.ClearM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Once);
            _mcPkgApiService.Verify(m => m.SetM02DatesAsync(plant, _invitation1.Id, project1, new List<string> { mcPkgNo1 }, new List<string>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingCommPkgTopic_ShouldFailIfEmpty()
        {

            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.CommPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingMcPkgTopic_ShouldFailIfEmpty()
        {

            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.McPkg, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingIpoTopic_ShouldFailIfEmpty()
        {

            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Ipo, message, new CancellationToken(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task HandlingProjectTopic_ShouldFailIfEmpty()
        {

            var message = $"{{}}";

            // Act
            await _dut.ProcessMessageAsync(PcsTopic.Project, message, new CancellationToken(false));
        }
    }
}
