using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.PersonAggregate
{
    [TestClass]
    public class PersonTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var p = new Person(new Guid("11111111-1111-2222-2222-333333333333"), "FirstName", "LastName", "UserName", "EmailAddress");

            Assert.AreEqual("11111111-1111-2222-2222-333333333333", p.Oid.ToString());
            Assert.AreEqual("FirstName", p.FirstName);
            Assert.AreEqual("LastName", p.LastName);
            Assert.AreEqual("UserName", p.UserName);
            Assert.AreEqual("EmailAddress", p.Email);
        }
    }
}
