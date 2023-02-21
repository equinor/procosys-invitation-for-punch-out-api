using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Equinor.ProCoSys.IPO.Infrastructure.Tests
{
    public class ContextHelper
    {
        public ContextHelper()
        {
            DbOptions = new DbContextOptions<IPOContext>();
            EventDispatcherMock = new Mock<IEventDispatcher>();
            PlantProviderMock = new Mock<IPlantProvider>();
            CurrentUserProviderMock = new Mock<ICurrentUserProvider>();
            ContextMock = new Mock<IPOContext>(
                DbOptions,
                PlantProviderMock.Object,
                EventDispatcherMock.Object,
                CurrentUserProviderMock.Object);
        }

        public DbContextOptions<IPOContext> DbOptions { get; }
        public Mock<IEventDispatcher> EventDispatcherMock { get; }
        public Mock<IPlantProvider> PlantProviderMock { get; }
        public Mock<IPOContext> ContextMock { get; }
        public Mock<ICurrentUserProvider> CurrentUserProviderMock { get; }
    }
}
