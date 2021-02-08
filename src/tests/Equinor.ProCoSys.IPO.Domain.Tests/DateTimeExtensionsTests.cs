using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests
{
    [TestClass]
    public class DateTimeExtensionsTests
    {
        private static readonly DateTime Monday = new DateTime(2020, 1, 6, 2, 3, 4, DateTimeKind.Utc);

        [TestMethod]
        public void StartOfWeek_ShouldGetStartOfWeekWhenDayIsMonday()
        {
            var startOfWeek = Monday.StartOfWeek();

            AssertStartOfWeek(startOfWeek);
        }

        [TestMethod]
        public void StartOfWeek_ShouldGetStartOfWeekWhenDayIsTuesday()
        {
            var tuesday = Monday.AddDays(1);

            var startOfWeek = tuesday.StartOfWeek();

            AssertStartOfWeek(startOfWeek);
        }

        [TestMethod]
        public void StartOfWeek_ShouldGetStartOfWeekWhenDayIsWednesday()
        {
            var wednesday = Monday.AddDays(2);

            var startOfWeek = wednesday.StartOfWeek();

            AssertStartOfWeek(startOfWeek);
        }

        [TestMethod]
        public void StartOfWeek_ShouldGetStartOfWeekWhenDayIsThursday()
        {
            var thursday = Monday.AddDays(3);

            var startOfWeek = thursday.StartOfWeek();

            AssertStartOfWeek(startOfWeek);
        }

        [TestMethod]
        public void StartOfWeek_ShouldGetStartOfWeekWhenDayIsFriday()
        {
            var friday = Monday.AddDays(4);

            var startOfWeek = friday.StartOfWeek();

            AssertStartOfWeek(startOfWeek);
        }

        [TestMethod]
        public void StartOfWeek_ShouldGetStartOfWeekWhenDayIsSaturday()
        {
            var saturday = Monday.AddDays(5);

            var startOfWeek = saturday.StartOfWeek();

            AssertStartOfWeek(startOfWeek);
        }

        [TestMethod]
        public void StartOfWeek_ShouldGetStartOfWeekWhenDayIsSunday()
        {
            var sunday = Monday.AddDays(6);

            var startOfWeek = sunday.StartOfWeek();

            AssertStartOfWeek(startOfWeek);
        }

        [TestMethod]
        public void Add2Weeks_ShouldAdd14Days()
        {
            var dt = new DateTime(2020, 1, 7, 2, 3, 4, DateTimeKind.Utc);

            dt = dt.AddWeeks(2);

            Assert.AreEqual(2020, dt.Year);
            Assert.AreEqual(1, dt.Month);
            Assert.AreEqual(21, dt.Day);
            Assert.AreEqual(2, dt.Hour);
            Assert.AreEqual(3, dt.Minute);
            Assert.AreEqual(4, dt.Second);
            Assert.AreEqual(DateTimeKind.Utc, dt.Kind);
        }

        private static void AssertStartOfWeek(DateTime dt)
        {
            Assert.AreEqual(Monday.Year, dt.Year);
            Assert.AreEqual(Monday.Month, dt.Month);
            Assert.AreEqual(Monday.Day, dt.Day);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(0, dt.Minute);
            Assert.AreEqual(0, dt.Second);
            Assert.AreEqual(DateTimeKind.Utc, dt.Kind);
        }
    }
}
