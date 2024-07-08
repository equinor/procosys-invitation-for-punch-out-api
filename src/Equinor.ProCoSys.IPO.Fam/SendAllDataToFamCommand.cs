using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Fam;

public class SendAllDataToFamCommand : IRequest<Result<string>>
{
}
