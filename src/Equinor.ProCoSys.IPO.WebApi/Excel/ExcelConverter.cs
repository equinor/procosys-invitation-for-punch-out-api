using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport;
using Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation;
using LargeXlsx;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Color = System.Drawing.Color;

namespace Equinor.ProCoSys.IPO.WebApi.Excel
{
    public class ExcelConverter : IExcelConverter
    {
        private readonly ILogger<ExcelConverter> _logger;

        public ExcelConverter(
            ILogger<ExcelConverter> logger) => _logger = logger;

        public MemoryStream Convert(ExportDto dto)
        {
            var stream = new MemoryStream();
            using var xlsxWriter = new XlsxWriter(stream);
            var headerStyle = new XlsxStyle(new XlsxFont("Carlito", 14, Color.Black, bold: true), XlsxStyle.Default.Fill, XlsxStyle.Default.Border, XlsxNumberFormat.General, XlsxAlignment.Default);
            var subHeaderStyle = new XlsxStyle(new XlsxFont("Carlito", 11, Color.Black, bold: true), XlsxStyle.Default.Fill, XlsxStyle.Default.Border, XlsxNumberFormat.General, XlsxAlignment.Default);
            var normalStyle = new XlsxStyle(new XlsxFont("Carlito", 11, Color.Black, bold: false), XlsxStyle.Default.Fill, XlsxStyle.Default.Border, XlsxNumberFormat.General, XlsxAlignment.Default);
            var invitationsHeader = new XlsxStyle(new XlsxFont("Carlito", 12, Color.Black, bold: true), XlsxStyle.Default.Fill, XlsxStyle.Default.Border, XlsxNumberFormat.General, XlsxAlignment.Default);
            var dateStyle = XlsxStyle.Default.With(XlsxNumberFormat.ShortDateTime);

            try
            {
                _logger.LogInformation($"Export to excel. Start generating sheets...");

                GenerateFrontSheet(dto, xlsxWriter, headerStyle, subHeaderStyle, normalStyle, dateStyle);

                var exportInvitationDtos = dto.Invitations.ToList();

                if (exportInvitationDtos.Any())
                {
                    _logger.LogInformation("Export to excel. Generating invitation sheet...");

                    GenerateInvitationsSheet(xlsxWriter, normalStyle, invitationsHeader, dateStyle,
                        exportInvitationDtos);

                    _logger.LogInformation("Export to excel. Generating participants sheet...");

                    GenerateParticipantsSheet(xlsxWriter, invitationsHeader, exportInvitationDtos, dateStyle,
                        normalStyle);
                    _logger.LogInformation("Export to excel. Generating history sheet...");

                    GenerateHistorySheet(xlsxWriter, normalStyle, invitationsHeader, exportInvitationDtos, dateStyle);
                }
                _logger.LogInformation("Export to excel. Completed.");

                return stream;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void GenerateHistorySheet(XlsxWriter xlsxWriter, XlsxStyle normalStyle, XlsxStyle invitationsHeader, List<ExportInvitationDto> exportInvitationDtos, XlsxStyle dateStyle)
        {
            if (exportInvitationDtos.Count != 1)
            {
                return;
            }

            var ipoWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.Id.ToString());
            var descriptionWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.Description);
            var createdByWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.CreatedBy);

            xlsxWriter.BeginWorksheet("History", columns: new[]
            {
                    XlsxColumn.Formatted(width: ipoWidth),
                    XlsxColumn.Formatted(width: descriptionWidth),
                    XlsxColumn.Formatted(width: 30),
                    XlsxColumn.Formatted(width: createdByWidth),
                }
            )
                .SetDefaultStyle(invitationsHeader)
                .BeginRow()
                .Write("Ipo nr")
                .Write("Description")
                .Write("Date (UTC)")
                .Write("User");

            var invitation = exportInvitationDtos.Single();

            foreach (var history in invitation.History)
            {
                xlsxWriter.SetDefaultStyle(normalStyle)
                    .BeginRow()
                    .Write(invitation.Id)
                .Write(history.Description)
                    .Write(history.CreatedAtUtc, dateStyle)
                    .Write(history.CreatedBy);

            }
        }

        private static void GenerateFrontSheet(ExportDto dto, XlsxWriter xlsxWriter, XlsxStyle headerStyle, XlsxStyle subHeaderStyle, XlsxStyle normalStyle, XlsxStyle dateStyle)
        {
            xlsxWriter.BeginWorksheet("Filters", columns: new[] { XlsxColumn.Formatted(width: 45), XlsxColumn.Formatted(width: 50) })
                                .SetDefaultStyle(headerStyle)
                                .BeginRow().Write("Export of invitation to punch-outs")
                                .SetDefaultStyle(subHeaderStyle)
                                .BeginRow().Write("Plant").Write(dto.UsedFilter.Plant)
                                .BeginRow().Write("Project name").Write(dto.UsedFilter.ProjectName)
                                .BeginRow().Write("Filter values:")
                                .SetDefaultStyle(normalStyle)
                                .BeginRow().Write("Ipo number starts with").Write(dto.UsedFilter.IpoIdStartsWith)
                                .BeginRow().Write("Title starts with").Write(dto.UsedFilter.IpoTitleStartWith)
                                .BeginRow().Write("CommPkg number starts with").Write(dto.UsedFilter.CommPkgNoStartWith)
                                .BeginRow().Write("McPkg number starts with").Write(dto.UsedFilter.McPkgNoStartsWith);
            if (dto.UsedFilter.PunchOutDateFromUtc != null)
            {
                xlsxWriter.BeginRow().Write("Punch out dates from").Write((DateTime)dto.UsedFilter.PunchOutDateToUtc, dateStyle);
            }
            else
            {
                xlsxWriter.BeginRow().Write("Punch out dates from");
            }

            if (dto.UsedFilter.PunchOutDateToUtc != null)
            {
                xlsxWriter.BeginRow().Write("Punch out dates to").Write((DateTime)dto.UsedFilter.PunchOutDateToUtc, dateStyle);
            }
            else
            {
                xlsxWriter.BeginRow().Write("Punch out dates to");
            }

            xlsxWriter.BeginRow().Write("Ipo status").Write(string.Join(",", dto.UsedFilter.IpoStatuses));

            if (dto.UsedFilter.LastChangedFromUtc != null)
            {
                xlsxWriter.BeginRow().Write("Last changed dates from").Write((DateTime)dto.UsedFilter.LastChangedFromUtc, dateStyle);
            }
            else
            {
                xlsxWriter.BeginRow().Write("Last changed dates from");
            }

            if (dto.UsedFilter.LastChangedToUtc != null)
            {
                xlsxWriter.BeginRow().Write("Last changed dates to").Write((DateTime)dto.UsedFilter.LastChangedToUtc, dateStyle);
            }
            else
            {
                xlsxWriter.BeginRow().Write("Last changed dates to");
            }

            xlsxWriter
                .BeginRow().Write("Functional role invited").Write(dto.UsedFilter.FunctionalRoleInvited)
                .BeginRow().Write("Person invited").Write(dto.UsedFilter.PersonInvited);
        }

        private static void GenerateParticipantsSheet(XlsxWriter xlsxWriter, XlsxStyle headerStyle, List<ExportInvitationDto> exportInvitationDtos, XlsxStyle dateStyle, XlsxStyle normalStyle)
        {
            var ipoWidth = exportInvitationDtos.OrderByDescending(s => s.Id.ToString().Length).FirstOrDefault().Id.ToString().Length + 10;
            var orgWidth = exportInvitationDtos
                .SelectMany(s => s.Participants)
                .OrderByDescending(a => a?.Organization?.Length ?? 40)
                .FirstOrDefault()?.Organization?.Length + 10 ?? 40;

            var typeWidth = exportInvitationDtos
                .SelectMany(s => s.Participants)
                .OrderByDescending(a => a?.Type?.Length ?? 40)
                .FirstOrDefault()?.Type?.Length + 10 ?? 40;

            var participantWidth = exportInvitationDtos
                .SelectMany(s => s.Participants)
                .OrderByDescending(a => a?.Participant?.Length ?? 40)
                .FirstOrDefault()?.Participant?.Length + 10 ?? 40;

            var noteWidth = exportInvitationDtos
                .SelectMany(s => s.Participants)
                .OrderByDescending(a => a?.Note?.Length ?? 40)
                .FirstOrDefault()?.Note?.Length + 10 ?? 40;

            var signedByWidth = exportInvitationDtos
                .SelectMany(s => s.Participants)
                .OrderByDescending(a => a?.SignedBy?.Length ?? 40)
                .FirstOrDefault()?.SignedBy?.Length + 10 ?? 40;

            // Participant sheet
            xlsxWriter.BeginWorksheet("Participants",
                columns: new[] {
                        XlsxColumn.Formatted(width: ipoWidth),
                        XlsxColumn.Formatted(width: orgWidth),
                        XlsxColumn.Formatted(width: typeWidth),
                        XlsxColumn.Formatted(width: participantWidth),
                        XlsxColumn.Formatted(width: 20),
                        XlsxColumn.Formatted(width: noteWidth),
                        XlsxColumn.Formatted(width: signedByWidth ),
                        XlsxColumn.Formatted(width: 20),
                })
                .SetDefaultStyle(headerStyle)
                .BeginRow()
                .Write("Ipo nr")
                .Write("Organization")
                .Write("Type")
                .Write("Participant")
                .Write("Attended")
                .Write("Note")
                .Write("SignedBy")
                .Write("SignedAtUtc");


            foreach (var invitation in exportInvitationDtos)
            {
                foreach (var participant in invitation.Participants)
                {
                    xlsxWriter.SetDefaultStyle(normalStyle)
                    .BeginRow()
                        .Write(invitation.Id)
                        .Write(participant.Organization)
                        .Write(participant.Type)
                        .Write(participant.Participant)
                        .Write(participant.Attended)
                        .Write(participant.Note)
                        .Write(participant.SignedBy);

                    if (participant.SignedAtUtc.HasValue)
                    {
                        xlsxWriter.Write(participant.SignedAtUtc.Value, dateStyle);
                    }
                }
            }
        }

        private static int CalculatePropertyWidth<T>(IEnumerable<T> collection, Func<T, string> propertySelector, int defaultValue = 20)
        {
            return collection
                .Select(propertySelector)
                .Where(prop => !string.IsNullOrEmpty(prop))
                .OrderByDescending(prop => prop.Length)
                .FirstOrDefault()?.Length + 10 ?? defaultValue;
        }

        private static void GenerateInvitationsSheet(XlsxWriter xlsxWriter, XlsxStyle normalStyle, XlsxStyle invitationsHeader, XlsxStyle dateStyle, List<ExportInvitationDto> exportInvitationDtos)
        {
            var ipoWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.Id.ToString());
            var projNameWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.ProjectName);
            var statusWidth = 20;
            var titleWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.Title);
            var descriptionWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.Description, 40);
            var locationWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.Location);
            var typeWidth = CalculatePropertyWidth(exportInvitationDtos, s => s.Type);
            var timeWidth = 20;

            var mcPckgsWidth = exportInvitationDtos
                .OrderByDescending(s => s.McPkgs?.Count() ?? 0)
                .FirstOrDefault()
                ?.McPkgs
                ?.Select(x => x.Length)
                .DefaultIfEmpty(0)
                .Max() + 10 ?? 20;

            var commPckgsWidth = exportInvitationDtos
                .OrderByDescending(s => s.CommPkgs?.Count() ?? 0)
                .FirstOrDefault()
                ?.CommPkgs
                ?.Select(x => x.Length)
                .DefaultIfEmpty(0)
                .Max() + 10 ?? 20;

            var contractorRepLength = CalculatePropertyWidth(exportInvitationDtos, s => s.ContractorRep);
            var constructionCompanyRefLength = CalculatePropertyWidth(exportInvitationDtos, s => s.ConstructionCompanyRep);
            var createdByLength = CalculatePropertyWidth(exportInvitationDtos, s => s.CreatedBy);

            // Define the time zone for GMT+1 (Central European Time)
            var cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

            xlsxWriter.BeginWorksheet("Invitations", columns: new[] {
                XlsxColumn.Formatted(width: ipoWidth),
                XlsxColumn.Formatted(width: projNameWidth),
                XlsxColumn.Formatted(width: statusWidth),
                XlsxColumn.Formatted(width: titleWidth),
                XlsxColumn.Formatted(width: descriptionWidth),
                XlsxColumn.Formatted(width: locationWidth),
                XlsxColumn.Formatted(width: typeWidth),
                XlsxColumn.Formatted(width: timeWidth),
                XlsxColumn.Formatted(width: timeWidth),
                XlsxColumn.Formatted(width: mcPckgsWidth),
                XlsxColumn.Formatted(width: commPckgsWidth),
                XlsxColumn.Formatted(width: contractorRepLength),
                XlsxColumn.Formatted(width: constructionCompanyRefLength),
                XlsxColumn.Formatted(width: timeWidth),
                XlsxColumn.Formatted(width: timeWidth),
                XlsxColumn.Formatted(width: createdByLength),
                XlsxColumn.Formatted(width: timeWidth)})
            .SetDefaultStyle(invitationsHeader)
            .BeginRow().Write("Ipo no")
            .Write("Project name")
            .Write("Status")
            .Write("Title")
            .Write("Description")
            .Write("Location")
            .Write("Type")
            .Write("Start time")
            .Write("End time")
            .Write("Mc pkgs")
            .Write("Comm pkgs")
            .Write("Contractor rep")
            .Write("Construction company rep")
            .Write("Completed at")
            .Write("Accepted at")
            .Write("Created by")
            .Write("Created at");

            foreach (var invitation in exportInvitationDtos)
            {
                xlsxWriter.SetDefaultStyle(normalStyle)
                .BeginRow()
                .Write(invitation.Id)
                .Write(invitation.ProjectName)
                .Write(invitation.Status.ToString())
                .Write(invitation.Title)
                .Write(invitation.Description)
                .Write(invitation.Location)
                .Write(invitation.Type);



                if (invitation.StartTimeUtc != DateTime.MinValue)
                {
                    // Convert the UTC time to GMT+1
                    var startTimeCet = TimeZoneInfo.ConvertTimeFromUtc(invitation.StartTimeUtc, cetTimeZone);
                    xlsxWriter.Write(startTimeCet, dateStyle);
                }
                else
                {
                    xlsxWriter.Write("");
                }

                if (invitation.EndTimeUtc != DateTime.MinValue)
                {
                    // Convert the UTC time to GMT+1
                    var endTimeCet = TimeZoneInfo.ConvertTimeFromUtc(invitation.EndTimeUtc, cetTimeZone);
                    xlsxWriter.Write(endTimeCet, dateStyle);
                }
                else
                {
                    xlsxWriter.Write("");
                }

                xlsxWriter
                .Write(string.Join(", ", invitation.McPkgs))
                .Write(string.Join(", ", invitation.CommPkgs))
                .Write(invitation.ContractorRep)
                .Write(invitation.ConstructionCompanyRep);

                if (invitation.CompletedAtUtc.HasValue)
                {
                    var completeAtCet = TimeZoneInfo.ConvertTimeFromUtc((DateTime)invitation.CompletedAtUtc, cetTimeZone);
                    xlsxWriter.Write(completeAtCet, dateStyle);
                }
                else
                {
                    xlsxWriter.Write("");
                }

                if (invitation.AcceptedAtUtc.HasValue)
                {
                    var acceptedAtCet = TimeZoneInfo.ConvertTimeFromUtc((DateTime)invitation.AcceptedAtUtc, cetTimeZone);
                    xlsxWriter.Write(acceptedAtCet, dateStyle);
                }
                else
                {
                    xlsxWriter.Write("");
                }

                var createdAtCet = TimeZoneInfo.ConvertTimeFromUtc(invitation.CreatedAtUtc, cetTimeZone);

                xlsxWriter.Write(invitation.CreatedBy)
                    .Write(createdAtCet, dateStyle);
            }
        }

        public string GetFileName() => $"InvitationForPunchOuts-{DateTime.Now:yyyyMMdd-hhmmss}";
    }
}
