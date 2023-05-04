﻿using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate
{
    public class SavedFilter : PlantEntityBase, ICreationAuditable, IModificationAuditable
    {
        public const int TitleLengthMax = 255;
        public const int CriteriaLengthMax = 8000;

        public SavedFilter() : base(null)
        {
        }

        public SavedFilter(string plant, Project project, string title, string criteria)
            : base(plant)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException(nameof(criteria));
            }
            if (project != null)
            {
                ProjectId = project.Id;
            }            
            Title = title;
            Criteria = criteria;
        }

        public int? ProjectId { get; set; }
        public string Title { get; set; }
        public string Criteria { get; set; }
        public bool DefaultFilter { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }

        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }

        public void SetModified(Person modifiedBy)
        {
            ModifiedAtUtc = TimeService.UtcNow;
            if (modifiedBy == null)
            {
                throw new ArgumentNullException(nameof(modifiedBy));
            }
            ModifiedById = modifiedBy.Id;
        }
    }
}
