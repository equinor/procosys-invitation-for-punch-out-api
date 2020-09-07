using Equinor.Procosys.CPO.Domain;
using Equinor.Procosys.CPO.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Equinor.Procosys.CPO.Infrastructure.Tests
{
    public class ContextHelper
    {
        public ContextHelper()
        {
            DbOptions = new DbContextOptions<CPOContext>();
            EventDispatcherMock = new Mock<IEventDispatcher>();
            PlantProviderMock = new Mock<IPlantProvider>();
            CurrentUserProviderMock = new Mock<ICurrentUserProvider>();
            ContextMock = new Mock<CPOContext>(DbOptions, PlantProviderMock.Object, EventDispatcherMock.Object, CurrentUserProviderMock.Object);
        }

        public DbContextOptions<CPOContext> DbOptions { get; }
        public Mock<IEventDispatcher> EventDispatcherMock { get; }
        public Mock<IPlantProvider> PlantProviderMock { get; }
        public Mock<CPOContext> ContextMock { get; }
        public Mock<ICurrentUserProvider> CurrentUserProviderMock { get; }
    }
}
