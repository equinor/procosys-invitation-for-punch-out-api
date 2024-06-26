﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.MessageContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.AddComment
{
    [TestClass]
    public class AddCommentCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IIntegrationEventPublisher> _integrationEventPublisherMock;

        private AddCommentCommand _command;
        private AddCommentCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private static readonly Project _project = new(_plant, _projectName, $"Description of {_projectName} project", _projectGuid);
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const string _commentText = "comment text";
        private const DisciplineType _typeDP = DisciplineType.DP;
        private Invitation _invitation;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _unitOfWorkMock = new Mock<IUnitOfWork>();

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
                new List<McPkg> { new McPkg(_plant, _project, "Comm", "Mc", "d", "1|2", Guid.Empty, Guid.Empty) },
                null);

            _integrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            //command
            _command = new AddCommentCommand(
                _invitation.Id,
                _commentText);

            _dut = new AddCommentCommandHandler(
                _plantProviderMock.Object,
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }

        [TestMethod]
        public async Task AddCommentCommand_ShouldAddComment()
        {
            Assert.AreEqual(0, _invitation.Comments.Count);

            await _dut.Handle(_command, default);

            Assert.AreEqual(1, _invitation.Comments.Count);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddCommentCommand_ShouldNotHaveDefaultGuid()
        {
            await _dut.Handle(_command, default);

            Assert.AreNotEqual(new Guid("00000000-0000-0000-0000-000000000000"), _invitation.Comments.First().Guid);
        }
    }
}
