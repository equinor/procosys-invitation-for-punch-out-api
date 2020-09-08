using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Events;
using Equinor.ProCoSys.IPO.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure
{
    public class IPOContext : DbContext, IUnitOfWork, IReadOnlyContext
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;

        public IPOContext(
            DbContextOptions<IPOContext> options,
            IPlantProvider plantProvider,
            IEventDispatcher eventDispatcher,
            ICurrentUserProvider currentUserProvider)
            : base(options)
        {
            _plantProvider = plantProvider;
            _eventDispatcher = eventDispatcher;
            _currentUserProvider = currentUserProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            SetGlobalPlantFilter(modelBuilder);
        }

        public static DateTimeKindConverter DateTimeKindConverter { get; } = new DateTimeKindConverter();
        public static NullableDateTimeKindConverter NullableDateTimeKindConverter { get; } = new NullableDateTimeKindConverter();

        
        public virtual DbSet<Person> Persons { get; set; }

        private void SetGlobalPlantFilter(ModelBuilder modelBuilder)
        {
            // Set global query filter on entities inheriting from PlantEntityBase
            // https://gunnarpeipman.com/ef-core-global-query-filters/
            foreach (var type in TypeProvider.GetEntityTypes(typeof(IDomainMarker).GetTypeInfo().Assembly, typeof(PlantEntityBase)))
            {
                typeof(IPOContext)
                .GetMethod(nameof(IPOContext.SetGlobalQueryFilter))
                ?.MakeGenericMethod(type)
                .Invoke(this, new object[] { modelBuilder });
            }
        }

        public void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : PlantEntityBase =>
            builder
            .Entity<T>()
            .HasQueryFilter(e => e.Plant == _plantProvider.Plant);

        public IQueryable<TEntity> QuerySet<TEntity>() where TEntity : class => Set<TEntity>().AsNoTracking();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await DispatchEventsAsync(cancellationToken);
            await SetAuditDataAsync();
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException concurrencyException)
            {
                throw new ConcurrencyException("Data store operation failed. Data may have been modified or deleted since entities were loaded.", concurrencyException);
            }
        }

        private async Task DispatchEventsAsync(CancellationToken cancellationToken = default)
        {
            var entities = ChangeTracker
                .Entries<EntityBase>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .Select(x => x.Entity);
            await _eventDispatcher.DispatchAsync(entities, cancellationToken);
        }

        private async Task SetAuditDataAsync()
        {
            var addedEntries = ChangeTracker
                .Entries<ICreationAuditable>()
                .Where(x => x.State == EntityState.Added)
                .ToList();
            var modifiedEntries = ChangeTracker
                .Entries<IModificationAuditable>()
                .Where(x => x.State == EntityState.Modified)
                .ToList();

            if (addedEntries.Any() || modifiedEntries.Any())
            {
                var currentUserOid = _currentUserProvider.GetCurrentUserOid();
                var currentUser = await Persons.SingleOrDefaultAsync(p => p.Oid == currentUserOid);

                foreach (var entry in addedEntries)
                {
                    entry.Entity.SetCreated(currentUser);
                }

                foreach (var entry in modifiedEntries)
                {
                    entry.Entity.SetModified(currentUser);
                }
            }
        }
    }
}
