using System;
using System.Net.Http;
using System.Text;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public class TestFile
    {
        private readonly byte[] _bytes;
        public string FileName { get; private set; }

        public TestFile(string content, string fileName)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            FileName = fileName;
            _bytes = Encoding.UTF8.GetBytes(content);
        }

        public HttpContent CreateHttpContent()
        {
            var bytes = new ByteArrayContent(_bytes);
            var multipartContent = new MultipartFormDataContent();
            var parameterName = "file";
            multipartContent.Add(bytes, parameterName, FileName);
            return multipartContent;
        }
    }
}
