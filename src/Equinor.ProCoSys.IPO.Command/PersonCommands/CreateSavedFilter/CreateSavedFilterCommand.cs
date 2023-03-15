﻿using Equinor.ProCoSys.Common;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter
{
    public class CreateSavedFilterCommand : IRequest<Result<int>>, IProjectRequest
    {
        public CreateSavedFilterCommand(
            string projectName,
            string title,
            string criteria,
            bool defaultFilter)
        {
            ProjectName = projectName;
            Title = title;
            Criteria = criteria;
            DefaultFilter = defaultFilter;
        }

        public string ProjectName { get; }
        public string Title { get; }
        public string Criteria { get; }
        public bool DefaultFilter { get; }
    }
}
