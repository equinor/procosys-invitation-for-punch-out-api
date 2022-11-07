using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate
{
    public class Person : EntityBase, IAggregateRoot, IModificationAuditable
    {
        public const int FirstNameLengthMax = 128;
        public const int LastNameLengthMax = 128;
        public const int UserNameLengthMax = 128;
        public const int EmailLengthMax = 128;

        private readonly List<SavedFilter> _savedFilters = new List<SavedFilter>();

        protected Person() : base()
        {
        }

        public Person(Guid oid, string firstName, string lastName, string userName, string email) : base()
        {
            Oid = oid;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            Email = email;
        }

        public IReadOnlyCollection<SavedFilter> SavedFilters => _savedFilters.AsReadOnly();
        public Guid Oid { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }

        public void SetModified(Person modifiedBy)
        {
            ModifiedAtUtc = TimeService.UtcNow;
            if (modifiedBy == null)
            {
                throw new ArgumentNullException(nameof(modifiedBy));
            }
            ModifiedById = modifiedBy.Id;
        }

        public void AddSavedFilter(SavedFilter savedFilter)
        {
            if (savedFilter == null)
            {
                throw new ArgumentNullException(nameof(savedFilter));
            }

            _savedFilters.Add(savedFilter);
        }

        public void RemoveSavedFilter(SavedFilter savedFilter)
        {
            if (savedFilter == null)
            {
                throw new ArgumentNullException(nameof(savedFilter));
            }

            _savedFilters.Remove(savedFilter);
        }

        public SavedFilter GetDefaultFilter(Project project) =>
            _savedFilters.SingleOrDefault(s => s.ProjectId == project.Id && s.DefaultFilter);

    }
}
