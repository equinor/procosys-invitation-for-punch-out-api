using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.PunchOut.WebApi.Controllers.CallOuts
{
    public class CallOutsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CallOutsController(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
