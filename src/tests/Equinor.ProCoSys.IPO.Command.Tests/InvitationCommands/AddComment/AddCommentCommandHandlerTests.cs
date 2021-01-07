﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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

        private AddCommentCommand _command;
        private AddCommentCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const string _commentText = "comment text";
        private const DisciplineType _type = DisciplineType.DP;
        private Invitation _invitation;
        private int _saveChangesCount;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => _saveChangesCount++);

            //create invitation
            _invitation = new Invitation(
                _plant,
                _projectName,
                _title,
                _description,
                _type,
                new DateTime(),
                new DateTime(),
                null);

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
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(1));
        }
    }
}
