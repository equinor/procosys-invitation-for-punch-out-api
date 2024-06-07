using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Fam;

public class SendAllDataToFamCommandHandler : IRequestHandler<SendAllDataToFamCommand, Result<string>>
{
    private readonly IFamRepository _famRepository;

    public SendAllDataToFamCommandHandler(IFamRepository famRepository)
    {
        _famRepository = famRepository;
    }

    public async Task<Result<string>> Handle(SendAllDataToFamCommand request, CancellationToken cancellationToken)
    {
        var participants = (await _famRepository.GetParticipants()).ToList();
        return new SuccessResult<string>("");
    }
}
