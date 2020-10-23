using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class UploadAttachmentDto
    {
        public IFormFile File { get; set; }
        public bool OverwriteIfExists { get; set; }
    }
}
