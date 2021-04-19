using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.Time;
using Equinor.ProCoSys.IPO.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Misc
{
    [ApiController]
    [Route("Email")]
    public class EmailController : ControllerBase
    {
        private IEmailService _emailService;

        public EmailController(IEmailService emailService) => _emailService = emailService;

        [AllowAnonymous]
        [HttpPut("Send")]
        public void Send()
        {
            _emailService.SendEmailsAsync(new List<string>() {"jehag@equinor.com"}, "test", "testtests");
        }
    }
}
