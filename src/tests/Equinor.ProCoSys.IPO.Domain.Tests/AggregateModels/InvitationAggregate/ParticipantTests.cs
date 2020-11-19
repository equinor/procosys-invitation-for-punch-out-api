using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.InvitationAggregate
{
    [TestClass]
    public class ParticipantTests
    {
        private const string TestPlant = "PlantA";
        private const Organization Org = Organization.Operation;
        private const IpoParticipantType Type = IpoParticipantType.Person;
        private const string FirstName = "Kari";
        private const string LastName = "Traa";
        private const string UserName = "KT";
        private const string Email = "kari@test.com";
        private Guid AzureOid = new Guid("11111111-1111-2222-2222-333333333333");

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        { 
            var dut = new Participant(
                TestPlant,
                Org,
                Type,
                null,
                FirstName,
                LastName,
                UserName,
                Email,
                AzureOid,
                0);
            Assert.AreEqual(TestPlant, dut.Plant);
            Assert.AreEqual(AzureOid, dut.AzureOid);
            Assert.AreEqual(FirstName, dut.FirstName);
            Assert.AreEqual(LastName, dut.LastName);
            Assert.AreEqual(UserName, dut.UserName);
            Assert.AreEqual(Email, dut.Email);
            Assert.AreEqual(Org, dut.Organization);
            Assert.AreEqual(Type, dut.Type);
            Assert.IsNull(dut.FunctionalRoleCode);
        }
    }
}
