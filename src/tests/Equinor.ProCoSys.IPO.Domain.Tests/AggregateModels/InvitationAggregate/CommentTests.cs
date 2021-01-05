using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.InvitationAggregate
{
    [TestClass]
    public class CommentTests
    {
        private Comment _dut;
        private const string TestPlant = "PlantA";
        private const string Comment = "CommentText";


        [TestInitialize]
        public void Setup() => _dut = new Comment(TestPlant, Comment);

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dut.Plant);
            Assert.AreEqual(Comment, _dut.CommentText);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenCommentTextNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new Comment(TestPlant, null)
            );
    }
}
