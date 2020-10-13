using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Query.GetPersonsInUserGroup;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetPersons
{
    public class GetPersonsQueryHandler : IRequestHandler<GetPersonsQuery, Result<List<ProCoSysPersonDto>>>
    {
        private readonly IPersonApiService _personApiService;
        private readonly IPlantProvider _plantProvider;

        public GetPersonsQueryHandler(
            IPersonApiService personApiService,
            IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
            _personApiService = personApiService;
        }

        public async Task<Result<List<ProCoSysPersonDto>>> Handle(
            GetPersonsQuery request,
            CancellationToken cancellationToken)
        {
            var mainApiPersons = await _personApiService
                .GetPersonsAsync(
                    _plantProvider.Plant,
                    request.SearchString)
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
