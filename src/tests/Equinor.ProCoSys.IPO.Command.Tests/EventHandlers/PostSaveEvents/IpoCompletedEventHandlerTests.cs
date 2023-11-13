using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.EventHandlers.PostSaveEvents
{
    [TestClass]
    public class IpoCompletedEventHandlerTests
    {
        private IpoCompletedEventHandler _dut;
        private Mock<ServiceBusSender> _serviceBusSender;
        private PcsBusSender _pcsBusSender;

        [TestInitialize]
        public void Setup()
        {
            _serviceBusSender = new Mock<ServiceBusSender>();
            _pcsBusSender = new PcsBusSender();
            _pcsBusSender.Add("ipo", _serviceBusSender.Object);

            _dut = new IpoCompletedEventHandler(_pcsBusSender);
        }

        [TestMethod]
        public async Task Handle_ShouldSendBusTopic()
        {
            // Arrange
            var sourceGuid = Guid.NewGuid();
            const string Plant = "TestPlant";
            var ipoCompletedEvent = new IpoCompletedEvent(Plant, sourceGuid);

            // Act
            await _dut.Handle(ipoCompletedEvent, default);

            // Assert
            _serviceBusSender.Verify(t => t.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [TestMethod]
        public async Task Handle_ShouldSendEmail()
        {
            // Arrange
            var sourceGuid = Guid.NewGuid();
            var plant = "TestPlant";
            var project = new Project("TestPlant", "project", "Description of project",Guid.NewGuid());
            var commpkgs = new List<CommPkg>
            {
                new CommPkg(plant, project, "commpkgno", "description", "status", "system|subsystem", Guid.Empty)
            };
            var invitation = new Invitation(plant, project, "title", "description", DisciplineType.MDP, DateTime.Now,
                DateTime.Now, "location", null, commpkgs);
            invitation.AddParticipant(new Participant(plant, Organization.ConstructionCompany,
                IpoParticipantType.Person, "code", "firstname", "lastname", "username", "email", Guid.NewGuid(), 1));
            var ipoCompletedEvent = new IpoCompletedEvent(plant, sourceGuid);

            // Act
            await _dut.Handle(ipoCompletedEvent, default);

        }
    }
}
