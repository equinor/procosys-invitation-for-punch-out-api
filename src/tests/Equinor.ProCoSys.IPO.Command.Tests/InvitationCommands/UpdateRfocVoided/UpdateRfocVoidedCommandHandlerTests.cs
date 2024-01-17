using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocVoidedStatus;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateRfocVoided
{
    [TestClass]
    public class UpdateRfocVoidedCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<ICertificateRepository> _certificateRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IProjectRepository> _projectRepositoryMock;
        private Mock<ILogger<UpdateRfocVoidedCommandHandler>> _loggerMock;
        private Mock<ICertificateApiService> _certificateApiServiceMock;

        private UpdateRfocVoidedCommand _command;
        private UpdateRfocVoidedCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName} project", _projectGuid);
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const DisciplineType _typeDP = DisciplineType.DP;
        private readonly Guid _certificateGuid = new Guid("11111111-2222-2222-2222-333333333333");
        private Invitation _invitation;
        private string _commPkgNo = "CommNo1";
        private string _commPkgNo2 = "CommNo2";
        private string _commPkgNo3 = "CommNo3";
        private string _mcPkgNo = "McNo1";
        private string _mcPkgNo2 = "McNo2";
        private string _mcPkgNo3 = "McNo3";
        private Certificate _certificate;
        private List<McPkg> _mcPkgs;
        private List<string> _mcPkgNos;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);
            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _certificateRepositoryMock = new Mock<ICertificateRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _certificateApiServiceMock = new Mock<ICertificateApiService>();
            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult<PCSCertificateCommPkgsModel>(null));
            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult<PCSCertificateMcPkgsModel>(null));

            var mcPkg1 = new McPkg(_plant, _project, "CommNo1", _mcPkgNo, "d", "1|2", Guid.Empty, Guid.Empty);
            var mcPkg2 = new McPkg(_plant, _project, "CommNo1", _mcPkgNo2, "d", "1|2", Guid.Empty, Guid.Empty);
            _mcPkgNos = new List<string> { _mcPkgNo, _mcPkgNo2 };

            _certificate = new Certificate(_plant, _project, _certificateGuid, true);
            _certificate.AddMcPkgRelation(mcPkg1);
            _certificate.AddMcPkgRelation(mcPkg2);
            _certificateRepositoryMock
                .Setup(c => c.GetCertificateByGuid(_certificateGuid))
                .Returns(Task.FromResult(_certificate));

            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectRepositoryMock
                .Setup(r => r.GetProjectOnlyByNameAsync(_projectName))
                .Returns(Task.FromResult(new Project(_plant, _projectName, "Desc", _projectGuid)));

            _loggerMock = new Mock<ILogger<UpdateRfocVoidedCommandHandler>>();

            _mcPkgs = new List<McPkg> { mcPkg1, mcPkg2 };
            
            //create invitation
            _invitation = new Invitation(
                    _plant,
                    _project,
                    _title,
                    _description,
                    _typeDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    _mcPkgs,
                    null);
            _invitation.ScopeHandedOver();

            //command
            _command = new UpdateRfocVoidedCommand(_projectName, _certificateGuid);

            _dut = new UpdateRfocVoidedCommandHandler(
                _invitationRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _plantProviderMock.Object,
                _certificateApiServiceMock.Object,
                _certificateRepositoryMock.Object,
                _loggerMock.Object);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_ShouldCallMainApi()
        {
            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _certificateApiServiceMock.Verify(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid), Times.Once);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_ShouldSetCertificateToVoided()
        {
            Assert.IsFalse(_certificate.IsVoided);
            Assert.AreEqual(IpoStatus.ScopeHandedOver, _invitation.Status);

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);
            Assert.IsTrue(_certificate.IsVoided);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_ShouldCallInvitationRepository()
        {
            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _invitationRepositoryMock.Verify(c => c.RfocVoidedHandling(_projectName, new List<string> { }, _mcPkgNos), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_ShouldCallSaveChanges()
        {
            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_ProjectNotFound_ShouldExitEarly()
        {
            _projectRepositoryMock
                .Setup(r => r.GetProjectOnlyByNameAsync(_projectName))
                .Returns(Task.FromResult<Project>(null));

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _certificateRepositoryMock.Verify(c => c.GetCertificateByGuid(_certificateGuid), Times.Never);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid), Times.Never);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid), Times.Never);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_ProjectIdClosed_ShouldExitEarly()
        {
            var project = new Project(_plant, _projectName, "Desc", _projectGuid);
            project.IsClosed = true;
            _projectRepositoryMock
                .Setup(r => r.GetProjectOnlyByNameAsync(_projectName))
                .Returns(Task.FromResult(project));

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _certificateRepositoryMock.Verify(c => c.GetCertificateByGuid(_certificateGuid), Times.Never);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid), Times.Never);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid), Times.Never);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_CertificateDoesNotExistInIPO_ShouldExitEarly()
        {
            _certificateRepositoryMock
                .Setup(c => c.GetCertificateByGuid(_certificateGuid))
                .Returns(Task.FromResult<Certificate>(null));

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _certificateRepositoryMock.Verify(c => c.GetCertificateByGuid(_certificateGuid), Times.Once);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid), Times.Never);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid), Times.Never);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_CertificateIsActiveInMainOnMcPkgCall_ShouldReturnUnexpectedResult()
        {

            var mcPkg1 = new PCSCertificateMcPkg
            {
                McPkgNo = _mcPkgNo,
                CommPkgNo = _commPkgNo
            };
            var mcPkg2 = new PCSCertificateMcPkg
            {
                McPkgNo = _mcPkgNo2,
                CommPkgNo = _commPkgNo
            };
            var mcPkg3 = new PCSCertificateMcPkg
            {
                McPkgNo = _mcPkgNo3,
                CommPkgNo = _commPkgNo3
            };
            var certificateMcPkgsModel = new PCSCertificateMcPkgsModel
            {
                CertificateIsAccepted = true,
                McPkgs = new List<PCSCertificateMcPkg> { mcPkg1, mcPkg2, mcPkg3 }
            };

            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult(certificateMcPkgsModel));

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Unexpected, result.ResultType);

            _certificateRepositoryMock.Verify(c => c.GetCertificateByGuid(_certificateGuid), Times.Once);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid), Times.Once);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid), Times.Never);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocVoidedCommand_CertificateIsActiveInMainOnCommPkgCall_ShouldReturnUnexpectedResult()
        {

            var commPkg1 = new PCSCertificateCommPkg
            {
                CommPkgNo = _commPkgNo
            };
            var commPkg2 = new PCSCertificateCommPkg
            {
                CommPkgNo = _commPkgNo2
            };
            var commPkg3 = new PCSCertificateCommPkg
            {
                CommPkgNo = _commPkgNo3
            };
            var certificateCommPkgsModel = new PCSCertificateCommPkgsModel
            {
                CertificateIsAccepted = true,
                CommPkgs = new List<PCSCertificateCommPkg> { commPkg1, commPkg2, commPkg3 }
            };

            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult(certificateCommPkgsModel));


            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Unexpected, result.ResultType);

            _certificateRepositoryMock.Verify(c => c.GetCertificateByGuid(_certificateGuid), Times.Once);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid), Times.Once);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid), Times.Once);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
