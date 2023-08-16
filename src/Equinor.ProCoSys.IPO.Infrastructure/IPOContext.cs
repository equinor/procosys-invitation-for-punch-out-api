using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.SettingAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using IDomainMarker = Equinor.ProCoSys.IPO.Domain.IDomainMarker;

namespace Equinor.ProCoSys.IPO.Infrastructure
{
    public class IPOContext : DbContext, IUnitOfWork, IReadOnlyContext
    {
        protected readonly IPlantProvider _plantProvider;
        protected readonly IEventDispatcher _eventDispatcher;
        protected readonly ICurrentUserProvider _currentUserProvider;

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
       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (DebugOptions.DebugEntityFrameworkInDevelopment)
            {
                optionsBuilder.LogTo(System.Console.WriteLine);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            SetGlobalPlantFilter(modelBuilder);
        }      

        public static DateTimeKindConverter DateTimeKindConverter { get; } = new DateTimeKindConverter();
        
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Invitation> Invitations { get; set; }
        public virtual DbSet<McPkg> McPkgs { get; set; }
        public virtual DbSet<CommPkg> CommPkgs { get; set; }
        public virtual DbSet<Participant> Participants { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<History> History { get; set; }
        public virtual DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<SavedFilter> SavedFilters { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }

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
            await DispatchDomainEventsEventsAsync(cancellationToken);
            await SetAuditDataAsync();
            UpdateConcurrencyToken();
            
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                await DispatchPostSaveEventsEventsAsync(cancellationToken);
                return result;
            }
            catch (DbUpdateConcurrencyException concurrencyException)
            {
                throw new ConcurrencyException("Data store operation failed. Data may have been modified or deleted since entities were loaded.", concurrencyException);
            }
        }
            
        public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default) 
            => await base.Database.BeginTransactionAsync(cancellationToken);

        public void Commit() => base.Database.CommitTransaction();

        protected virtual void UpdateConcurrencyToken()
        {
            var modifiedEntries = ChangeTracker
                .Entries<EntityBase>()
                .Where(x => x.State == EntityState.Modified || x.State == EntityState.Deleted);

            foreach (var entry in modifiedEntries)
            {
                var currentRowVersion = entry.CurrentValues.GetValue<byte[]>(nameof(EntityBase.RowVersion));
                var originalRowVersion = entry.OriginalValues.GetValue<byte[]>(nameof(EntityBase.RowVersion));
                for (var i = 0; i < currentRowVersion.Length; i++)
                {
                    originalRowVersion[i] = currentRowVersion[i];
                }
            }
        }

        private async Task DispatchDomainEventsEventsAsync(CancellationToken cancellationToken = default)
        {
            var entities = ChangeTracker
                .Entries<EntityBase>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .Select(x => x.Entity);
            await _eventDispatcher.DispatchDomainEventsAsync(entities, cancellationToken);
        }

        private async Task DispatchPostSaveEventsEventsAsync(CancellationToken cancellationToken = default)
        {
            var entities = ChangeTracker
                .Entries<EntityBase>()
                .Where(x => x.Entity.PostSaveDomainEvents != null && x.Entity.PostSaveDomainEvents.Any())
                .Select(x => x.Entity);
            await _eventDispatcher.DispatchPostSaveEventsAsync(entities, cancellationToken);
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
                var currentUser = await Persons.SingleOrDefaultAsync(p => p.Guid == currentUserOid);

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
