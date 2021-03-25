using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Query.GetPersons;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetPersonsWithPrivileges
{
    public class GetPersonsWithPrivilegesQueryHandler : IRequestHandler<GetPersonsWithPrivilegesQuery, Result<List<ProCoSysPersonDto>>>
    {
        private readonly IMainPersonApiService _personApiService;
        private readonly IPlantProvider _plantProvider;

        public GetPersonsWithPrivilegesQueryHandler(
            IMainPersonApiService personApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _personApiService = personApiService;
        }

        public async Task<Result<List<ProCoSysPersonDto>>> Handle(
            GetPersonsWithPrivilegesQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiPersons = await _personApiService
                .GetPersonsWithPrivilegesAsync(
                   _plantProvider.Plant,
                   request.SearchString,
                   request.ObjectName,
                   request.Privileges)
                ?? new List<ProCoSysPerson>();

            var personDtos = mainApiPersons
                .Select(person => new ProCoSysPersonDto(
                    person.AzureOid,
                    person.UserName,
                    person.FirstName,
                    person.LastName,
                    person.Email)).ToList();

            return new SuccessResult<List<ProCoSysPersonDto>>(personDtos);
        }
    }
}
