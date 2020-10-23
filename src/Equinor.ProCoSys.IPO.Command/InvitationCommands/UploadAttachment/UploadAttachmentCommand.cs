using System;
using System.IO;
using System.Text.Json.Serialization;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment
{
    public class UploadAttachmentCommand : IDisposable, IRequest<Result<int>>
    {
        private bool _disposedValue;

        public UploadAttachmentCommand(int invitationId, string fileName, bool overWriteIfExists, Stream content)
        {
            InvitationId = invitationId;
            FileName = fileName;
            OverWriteIfExists = overWriteIfExists;
            Content = content;
        }

        // JsonIgnore needed here so GlobalExceptionHandler do not try to serialize the Stream when reporting validation errors. 
        [JsonIgnore]
        public Stream Content { get; }
        public int InvitationId { get; }
        public string FileName { get; }
        public bool OverWriteIfExists { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Content.Dispose();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
