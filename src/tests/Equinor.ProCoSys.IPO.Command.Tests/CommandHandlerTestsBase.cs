﻿using System;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.Auth.Time;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Auth.Misc;

namespace Equinor.ProCoSys.IPO.Command.Tests
{
    [TestClass]
    public abstract class CommandHandlerTestsBase
    {
        protected readonly Guid CurrentUserOid = new Guid("12345678-1234-1234-1234-123456789123");
        protected Mock<ICurrentUserProvider> CurrentUserProviderMock;
        protected const string TestPlant = "TestPlant";
        protected Mock<IUnitOfWork> UnitOfWorkMock;
        protected Mock<IPlantProvider> PlantProviderMock;
        protected ManualTimeProvider _timeProvider;
        protected DateTime _utcNow;

        [TestInitialize]
        public void BaseSetup()
        {
            CurrentUserProviderMock = new Mock<ICurrentUserProvider>();
            CurrentUserProviderMock
                .Setup(x => x.GetCurrentUserOid())
                .Returns(CurrentUserOid);
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            PlantProviderMock = new Mock<IPlantProvider>();
            PlantProviderMock
                .Setup(x => x.Plant)
                .Returns(TestPlant);
            _utcNow = new DateTime(2020, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            _timeProvider = new ManualTimeProvider(_utcNow);
            TimeService.SetProvider(_timeProvider);
        }
    }
}
