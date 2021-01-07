using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetComments;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetComments
{
    [TestClass]
    public class GetCommentsQueryHandlerTests : ReadOnlyTestsBase
    {
        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = new Invitation(
                    TestPlant,
                    "TestProject",
                    "TestInvitation",
                    "TestDescription",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null);
                var comment = new Comment(TestPlant, "comment text");
                invitation.AddComment(comment);
                context.Invitations.Add(invitation);
                context.SaveChangesAsync().Wait();
            }
        }

        [TestMethod]
        public async Task Handle_ReturnsCorrectNumberOfComments()
        {
            using var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var query = new GetCommentsQuery(1);
            var dut = new GetCommentsQueryHandler(context);

            var result = await dut.Handle(query, default);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestMethod]
        public async Task Handle_ReturnsCorrectCommentDetails()
        {
            using var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var query = new GetCommentsQuery(1);
            var dut = new GetCommentsQueryHandler(context);

            var result = await dut.Handle(query, default);

            Assert.IsNotNull(result);
            Assert.AreEqual("comment text", result.Data[0].Comment);
            Assert.IsTrue(result.Data[0].Id > 0);
            Assert.IsNotNull(result.Data[0].CreatedAtUtc);
            Assert.IsNotNull(result.Data[0].CreatedBy);
        }
    }
}
