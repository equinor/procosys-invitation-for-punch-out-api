using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.CreateInvitation;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public static class InvitationsControllerTestsHelper
    {
        private const string Route = "Invitations";

        public static async Task<InvitationResultsDto> GetInvitationsAsync(
            UserType userType,
            string plant,
            string projectName,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection
            {
                {"ProjectName", projectName}
            };
            var url = $"{Route}{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<InvitationResultsDto>(jsonString);
        }

        public static async Task<XLFile> ExportInvitationsAsync(
            UserType userType,
            string plant,
            string projectName,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection
            {
                {"ProjectName", projectName}
            };
            var url = $"{Route}/ExportInvitationsToExcel{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var result = new XLFile
            {
                Workbook = new XLWorkbook(stream),
                ContentType = response.Content.Headers.ContentType?.MediaType
            };

            return result;
        }

        public static async Task<InvitationDto> GetInvitationAsync(
            UserType userType, string plant,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{id}");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<InvitationDto>(content);
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
            var url = $"{Route}/ByCommPkgNo/{commPkgNo}{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<InvitationForMainDto>>(content);
        }

        public static async Task<List<CommPkgWithMdpIposDto>> GetLatestMdpIpoOnCommPkgsAsync(
            UserType userType,
            string plant,
            IList<string> commPkgNos,
            string projectName,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection { { "projectName", projectName }};
            foreach (var commPkgNo in commPkgNos)
            {
                parameters.Add("commPkgNos", commPkgNo);
            }
            var url = $"{Route}/ByCommPkgNos{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CommPkgWithMdpIposDto>>(content);
        }

        public static async Task<List<AttachmentDto>> GetAttachmentsAsync(
            UserType userType, 
            string plant,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{id}/Attachments");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AttachmentDto>>(content);
        }

        public static async Task<List<HistoryDto>> GetHistoryAsync(
            UserType userType,
            string plant,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{id}/History");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<HistoryDto>>(content);
        }

        public static async Task<AttachmentDto> GetAttachmentAsync(
            UserType userType, 
            string plant,
            int id,
            int attachmentId,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/{id}/Attachments/{attachmentId}");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AttachmentDto>(content);
        }

        public static async Task<string> SignPunchOutAsync(
            UserType userType,
            string plant,
            int id,
            int participantId,
            string participantRowVersion,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                participantId,
                participantRowVersion
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/{id}/Sign", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> UnsignPunchOutAsync(
            UserType userType,
            string plant,
            int id,
            int participantId,
            string participantRowVersion,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                participantId,
                participantRowVersion
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/{id}/Unsign", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> CompletePunchOutAsync(
            UserType userType,
            string plant,
            int id,
            CompletePunchOutDto dto,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                dto.ParticipantRowVersion,
                dto.InvitationRowVersion,
                dto.Participants
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .PutAsync($"{Route}/{id}/Complete", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> UnCompletePunchOutAsync(
            UserType userType,
            string plant,
            int id,
            UnCompletePunchOutDto dto,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                dto.InvitationRowVersion,
                dto.ParticipantRowVersion
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .PutAsync($"{Route}/{id}/UnComplete", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> AcceptPunchOutAsync(
            UserType userType,
            string plant,
            int id,
            AcceptPunchOutDto dto,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new 
            {
                dto.ParticipantRowVersion,
                dto.InvitationRowVersion,
                dto.Participants
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .PutAsync($"{Route}/{id}/Accept", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> UnAcceptPunchOutAsync(
            UserType userType,
            string plant,
            int id,
            UnAcceptPunchOutDto dto,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                dto.InvitationRowVersion,
                dto.ParticipantRowVersion
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .PutAsync($"{Route}/{id}/UnAccept", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task ChangeAttendedStatusOnParticipantsAsync(
            UserType userType,
            string plant,
            int id,
            ParticipantToChangeDto[] dtos,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var serializePayload = JsonConvert.SerializeObject(dtos);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .PutAsync($"{Route}/{id}/AttendedStatusAndNotes", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
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
            IList<CreateParticipantsDto> participants,
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
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync(Route, content);

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
            EditInvitation.EditInvitedInvitationDto dto,
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

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/{id}", content);

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
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{id}/Attachments", httpContent);

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
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{id}/Attachments/{attachmentId}")
            {
                Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
            };

            var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
        }

        public static async Task<List<CommentDto>> GetCommentsAsync(
            UserType userType,
            string plant,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var url = $"{Route}/{id}/Comments";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CommentDto>>(content);
        }

        public static async Task<int> AddCommentAsync(
            UserType userType,
            string plant,
            int id,
            string comment,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                comment
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/{id}/Comments", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return -1;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<int>(jsonString);
        }

        public static async Task<string> CancelPunchOutAsync(
            UserType userType,
            string plant,
            int id,
            CancelPunchOutDto dto,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                dto.RowVersion
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .PutAsync($"{Route}/{id}/Cancel", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }


        public static async Task DeletePunchOutAsync(
            UserType userType,
            string plant,
            int id,
            string rowVersion,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                rowVersion
            };
            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{Route}/{id}/Delete")
            {
                Content = new StringContent(serializePayload, Encoding.UTF8, "application/json")
            };

            var response = await TestFactory.Instance.GetHttpClient(userType, plant).SendAsync(request);
            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
        }
    }
}
