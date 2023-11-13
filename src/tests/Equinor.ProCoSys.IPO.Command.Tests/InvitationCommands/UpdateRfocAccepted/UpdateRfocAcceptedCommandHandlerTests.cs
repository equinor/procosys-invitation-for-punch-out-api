using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocAcceptedStatus;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.UpdateRfocAccepted
{
    [TestClass]
    public class UpdateRfocAcceptedCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<ICertificateRepository> _certificateRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IProjectRepository> _projectRepositoryMock;
        private Mock<ILogger<UpdateRfocAcceptedCommandHandler>> _loggerMock;
        private Mock<ICertificateApiService> _certificateApiServiceMock;
        private Mock<ICommPkgApiService> _commPkgApiServiceMock;
        private Mock<IMcPkgApiService> _mcPkgApiServiceMock;
        private Mock<IDbContextTransaction> _transactionMock;

        private UpdateRfocAcceptedCommand _command;
        private UpdateRfocAcceptedCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName} project", _projectGuid);
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const DisciplineType _typeDP = DisciplineType.DP;
        private readonly Guid _certificateGuid = new Guid("11111111-2222-2222-2222-333333333333");
        private Invitation _invitation;
        private PCSCertificateCommPkgsModel _certificateCommPkgsModel;
        private PCSCertificateMcPkgsModel _certificateMcPkgsModel;
        private string _commPkgNo = "CommNo1";
        private string _commPkgNo2 = "CommNo2";
        private string _commPkgNo3 = "CommNo3";
        private string _mcPkgNo = "McNo1";
        private string _mcPkgNo2 = "McNo2";
        private string _mcPkgNo3 = "McNo3";
        private Certificate _createdCertificate;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);
            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();
            _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();
            _certificateRepositoryMock = new Mock<ICertificateRepository>();
            _certificateRepositoryMock
                .Setup(x => x.Add(It.IsAny<Certificate>()))
                .Callback<Certificate>(x => _createdCertificate = x);
            _transactionMock = new Mock<IDbContextTransaction>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_transactionMock.Object));

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
            _certificateCommPkgsModel = new PCSCertificateCommPkgsModel
            {
                CertificateIsAccepted = true,
                CommPkgs = new List<PCSCertificateCommPkg> { commPkg1, commPkg2, commPkg3 }
            };

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
            _certificateMcPkgsModel = new PCSCertificateMcPkgsModel
            {
                CertificateIsAccepted = true,
                McPkgs = new List<PCSCertificateMcPkg> { mcPkg1, mcPkg2, mcPkg3 }
            };

            _certificateApiServiceMock = new Mock<ICertificateApiService>();
            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult(_certificateCommPkgsModel));
            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult(_certificateMcPkgsModel));

            var mcPkgDetails = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D2", Id = 2, McPkgNo = _mcPkgNo, System = "1|2", OperationHandoverStatus = "ACCEPTED" };
            var mcPkgDetails2 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D3", Id = 3, McPkgNo = _mcPkgNo2, System = "1|2", OperationHandoverStatus = "SENT" };
            var mcPkgDetails3 = new ProCoSysMcPkg { CommPkgNo = _commPkgNo, Description = "D4", Id = 4, McPkgNo = _mcPkgNo3, System = "1|2", OperationHandoverStatus = "ACCEPTED" };
            IList<ProCoSysMcPkg> pcsMcPkgs = new List<ProCoSysMcPkg> { mcPkgDetails, mcPkgDetails2, mcPkgDetails3 };

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, new List<string> { _mcPkgNo, _mcPkgNo2, _mcPkgNo3 }))
                .Returns(Task.FromResult(pcsMcPkgs));

            var commPkgDetails = new ProCoSysCommPkg { CommPkgNo = _commPkgNo, Description = "D2", Id = 2, System = "1|2", OperationHandoverStatus = "SENT" };
            var commPkgDetails2 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo2, Description = "D2", Id = 2, System = "1|2", OperationHandoverStatus = "ACCEPTED" };
            var commPkgDetails3 = new ProCoSysCommPkg { CommPkgNo = _commPkgNo3, Description = "D2", Id = 2, System = "1|2", OperationHandoverStatus = "ACCEPTED" };
            IList<ProCoSysCommPkg> pcsCommPkgs = new List<ProCoSysCommPkg> { commPkgDetails, commPkgDetails2, commPkgDetails3 };

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, new List<string> { _commPkgNo, _commPkgNo2, _commPkgNo3 }))
                .Returns(Task.FromResult(pcsCommPkgs));

            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectRepositoryMock
                .Setup(r => r.GetProjectOnlyByNameAsync(_projectName))
                .Returns(Task.FromResult(new Project(_plant, _projectName, "Desc", _projectGuid)));


            _loggerMock = new Mock<ILogger<UpdateRfocAcceptedCommandHandler>>();

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
                    new List<McPkg> { new McPkg(_plant, _project, "CommNo1", "McNo1", "d", "1|2", Guid.Empty), new McPkg(_plant, _project, "CommNo1", "McNo2", "d", "1|2", Guid.Empty) },
                    null);

            //command
            _command = new UpdateRfocAcceptedCommand(_projectName, _certificateGuid);

            _dut = new UpdateRfocAcceptedCommandHandler(
                _invitationRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _plantProviderMock.Object,
                _certificateApiServiceMock.Object,
                _loggerMock.Object,
                _certificateRepositoryMock.Object,
                _mcPkgApiServiceMock.Object,
                _commPkgApiServiceMock.Object);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocStatusCommand_ShouldCallMainApi()
        {
            Assert.AreNotEqual(IpoStatus.ScopeHandedOver, _invitation.Status);

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _certificateApiServiceMock.Verify(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid), Times.Once);
            _certificateApiServiceMock.Verify(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocStatusCommand_ShouldCallSaveChanges()
        {
            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            Assert.IsNotNull(_createdCertificate);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocStatusCommand_ShouldNotSaveChangesWhenAcceptedCertificateIsFalse()
        {
            _certificateCommPkgsModel.CertificateIsAccepted = false;
            _certificateMcPkgsModel.CertificateIsAccepted = false;
            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateCommPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult(_certificateCommPkgsModel));
            _certificateApiServiceMock
                .Setup(c => c.TryGetCertificateMcPkgsAsync(_plant, _certificateGuid))
                .Returns(Task.FromResult(_certificateMcPkgsModel));

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocStatusCommand_ShouldCreateCertificateAndCreateRelation()
        {
            var mcPkgs = new List<McPkg> {
                new McPkg(_plant, _project, _commPkgNo, _mcPkgNo, "description", "1|2", Guid.Empty),
                new McPkg(_plant, _project, _commPkgNo, _mcPkgNo2, "description", "1|2", Guid.Empty),
                new McPkg(_plant, _project, _commPkgNo, _mcPkgNo3, "description", "1|2", Guid.Empty)
            };
            _invitationRepositoryMock
                .Setup(r => r.GetMcPkgs(_projectName, new List<string> { _mcPkgNo, _mcPkgNo2, _mcPkgNo3 }))
                .Returns(mcPkgs);

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(_createdCertificate);
            Assert.AreEqual(3, _createdCertificate.CertificateMcPkgs.Count);
            Assert.AreEqual(0, _createdCertificate.CertificateCommPkgs.Count);
        }

        [TestMethod]
        public async Task HandlingUpdateRfocAcceptedCommand_CertificateScopeDoesNotExistInIPO_ShouldExitEarly()
        {
            IList<ProCoSysMcPkg> pcsMcPkgs = new List<ProCoSysMcPkg>();

            _mcPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(_plant, _projectName, new List<string> { _mcPkgNo, _mcPkgNo2, _mcPkgNo3 }))
                .Returns(Task.FromResult(pcsMcPkgs));

            IList<ProCoSysCommPkg> pcsCommPkgs = new List<ProCoSysCommPkg>();

            _commPkgApiServiceMock
                .Setup(x => x.GetCommPkgsByCommPkgNosAsync(_plant, _projectName, new List<string> { _commPkgNo, _commPkgNo2, _commPkgNo3 }))
                .Returns(Task.FromResult(pcsCommPkgs));

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);

            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
