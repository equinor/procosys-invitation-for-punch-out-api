using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public static class InvitationsControllerTestsHelper
    {
        private const string _route = "Invitations";
        
        public static async Task<InvitationDetailsDto> GetInvitationAsync(
            UserType userType, string plant,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{id}");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<InvitationDetailsDto>(content);
        }

        public static async Task<List<InvitationForMainDto>> GetInvitationsByCommPkgNoAsync(
            UserType userType, 
            string plant,
            string commPkgNo,
            string projectName,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection { { "projectName", projectName } };
            var url = $"/ByCommPkgNo/{commPkgNo}{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<InvitationForMainDto>>(content);
        }

        public static async Task<List<AttachmentDto>> GetAttachmentsAsync(
            UserType userType, 
            string plant,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{id}/Attachments");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AttachmentDto>>(content);
        }

        public static async Task<AttachmentDto> GetAttachmentAsync(
            UserType userType, 
            string plant,
            int id,
            int attachmentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{_route}/{id}/Attachments/{attachmentId}");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AttachmentDto>(content);
        }

        public static async Task<int> CreateInvitationAsync(
            UserType userType, 
            string plant,
            string title,
            string description,
            string location,
            DisciplineType type,
            System.DateTime startTime,
            System.DateTime endTime,
            IList<ParticipantsForCommand> participants,
            IEnumerable<string> mcPkgScope,
            IEnumerable<string> commPkgScope,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new 
            {
                projectName = KnownTestData.ProjectName,
                title,
                description,
                location,
                type,
                startTime,
                endTime,
                participants,
                mcPkgScope,
                commPkgScope
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync(_route, content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return -1;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<int>(jsonString);
        }

        public static async Task<string> EditInvitationAsync(
            UserType userType, 
            string plant,
            int id,
            EditInvitationDto dto,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                dto.Title,
                dto.Description,
                dto.Location,
                dto.StartTime,
                dto.EndTime,
                dto.ProjectName,
                dto.Type,
                dto.UpdatedCommPkgScope,
                dto.UpdatedMcPkgScope,
                dto.UpdatedParticipants,
                dto.RowVersion
            };

            //todo figure out how to get remaining parameters in response url
            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{_route}/{id}", content);
            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task UploadAttachmentAsync(
            UserType userType, 
            string plant,
            int id,
            TestFile file,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var httpContent = file.CreateHttpContent();
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{_route}/{id}/Attachments", httpContent);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
        }

        public static async Task DeleteAttachmentAsync(
            UserType userType,
            string plant,
            int id,
            int attachmentId,
            string rowVersion,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                rowVersion
            };
            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_route}/{id}/Attachments/{attachmentId}")
            {
                Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
            };

            var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
        }
    }
}
