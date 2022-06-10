using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport;

namespace Equinor.ProCoSys.IPO.WebApi.Excel
{
    public class ExcelConverter : IExcelConverter
    {
        public static class FrontSheetRows
        {
            public static int MainHeading = 1;
            public static int Plant = 2;
            public static int ProjectName = 3;
            public static int FilterHeading = 4;
            public static int Invitation = 5;
            public static int Title = 6;
            public static int CommPkg = 7;
            public static int McPkg = 8;
            public static int PunchOutDateFrom = 9;
            public static int PunchOutDateTo = 10;
            public static int Status = 11;
            public static int LastChangedFrom = 12;
            public static int LastChangedTo = 13;
            public static int Role = 14;
            public static int Person = 15;
        }

        public static class InvitationSheetColumns
        {
            public static int IpoNo = 1;
            public static int Status = 2;
            public static int Title = 3;
            public static int Description = 4;
            public static int Location = 5;
            public static int Type = 6;
            public static int StartTimeUtc = 7;
            public static int EndTimeUtc = 8;
            public static int McPkgs = 9;
            public static int CommPkgs = 10;
            public static int ContractorRep = 11;
            public static int ConstructionCompanyRep = 12;
            public static int CompletedAtUtc = 13;
            public static int AcceptedAtUtc = 14;
            public static int CreatedAtUtc = 15;
            public static int CreatedBy = 16;
            public static int Last = CreatedBy;
        }

        public static class ParticipantsSheetColumns
        {
            public static int IpoNo = 1;
            public static int Organization = 2;
            public static int Type = 3;
            public static int Participant = 4;
            public static int Attended = 5;
            public static int Note = 6;
            public static int SignedBy = 7;
            public static int SignedAtUtc = 8;
            public static int Last = SignedAtUtc;
        }

        public static class HistorySheetColumns
        {
            public static int IpoNo = 1;
            public static int Description = 2;
            public static int Date = 3;
            public static int User = 4;
            public static int Last = User;
        }

        public MemoryStream Convert(ExportDto dto)
        {
            // see https://github.com/ClosedXML/ClosedXML for sample code
            var excelStream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                CreateFrontSheet(workbook, dto.UsedFilter);

                var exportInvitationDtos = dto.Invitations.ToList();

                CreateInvitationSheet(workbook, exportInvitationDtos);

                CreateParticipantsSheet(workbook, exportInvitationDtos);

                CreateHistorySheet(workbook, exportInvitationDtos);

                workbook.SaveAs(excelStream);
            }

            return excelStream;
        }

        private void AddDateCell(IXLRow row, int cellIdx, DateTime date, bool onlyDate = true)
        {
            var cell = row.Cell(cellIdx);

            if (date != DateTime.MinValue)
            {
                cell.SetValue(date).SetDataType(XLDataType.DateTime);
            }

            var format = "yyyy-mm-dd";
            if (!onlyDate)
            {
                format += " hh:mm";
            }
            cell.Style.DateFormat.Format = format;
        }

        private void CreateHistorySheet(XLWorkbook workbook, IList<ExportInvitationDto> invitations)
        {
            if (invitations.Count != 1)
            {
                return;
            }

            var sheet = workbook.Worksheets.Add("History");

            var rowIdx = 0;
            var row = sheet.Row(++rowIdx);
            row.Style.Font.SetBold();
            row.Style.Font.SetFontSize(12);
            row.Cell(HistorySheetColumns.IpoNo).Value = "Ipo nr";
            row.Cell(HistorySheetColumns.Description).Value = "Description";
            row.Cell(HistorySheetColumns.Date).Value = "Date (UTC)";
            row.Cell(HistorySheetColumns.User).Value = "User";

            var invitation = invitations.Single();
            foreach (var history in invitation.History)
            {
                row = sheet.Row(++rowIdx);

                row.Cell(HistorySheetColumns.IpoNo).SetValue(invitation.Id).SetDataType(XLDataType.Text);
                row.Cell(HistorySheetColumns.Description).SetValue(history.Description).SetDataType(XLDataType.Text);
                AddDateCell(row, HistorySheetColumns.Date, history.CreatedAtUtc);
                row.Cell(HistorySheetColumns.User).SetValue(history.CreatedBy).SetDataType(XLDataType.Text);
            }

            const int minWidth = 10;
            const int maxWidth = 100;
            sheet.Columns(1, HistorySheetColumns.Last).AdjustToContents(1, rowIdx, minWidth, maxWidth);
        }

        private void CreateParticipantsSheet(XLWorkbook workbook, IList<ExportInvitationDto> invitations)
        {
            var severalParticipantsSheet = workbook.Worksheets.Add("Participants");

            var rowIdx = 0;
            var row = severalParticipantsSheet.Row(++rowIdx);
            row.Style.Font.SetBold();
            row.Style.Font.SetFontSize(12);
            row.Cell(ParticipantsSheetColumns.IpoNo).Value = "Ipo nr";
            row.Cell(ParticipantsSheetColumns.Organization).Value = "Organization";
            row.Cell(ParticipantsSheetColumns.Type).Value = "Type";
            row.Cell(ParticipantsSheetColumns.Participant).Value = "Participant";
            row.Cell(ParticipantsSheetColumns.Attended).Value = "Attended";
            row.Cell(ParticipantsSheetColumns.Note).Value = "Note";
            row.Cell(ParticipantsSheetColumns.SignedBy).Value = "SignedBy";
            row.Cell(ParticipantsSheetColumns.SignedAtUtc).Value = "SignedAtUtc";

            foreach (var invitation in invitations)
            {

                foreach (var participant in invitation.Participants)
                {
                    row = severalParticipantsSheet.Row(++rowIdx);

                    row.Cell(ParticipantsSheetColumns.IpoNo).SetValue(invitation.Id).SetDataType(XLDataType.Text);
                    row.Cell(ParticipantsSheetColumns.Organization).SetValue(participant.Organization)
                        .SetDataType(XLDataType.Text);
                    row.Cell(ParticipantsSheetColumns.Type).SetValue(participant.Type).SetDataType(XLDataType.Text);
                    row.Cell(ParticipantsSheetColumns.Participant).SetValue(participant.Participant)
                        .SetDataType(XLDataType.Text);
                    row.Cell(ParticipantsSheetColumns.Attended).SetValue(participant.Attended)
                        .SetDataType(XLDataType.Boolean);
                    row.Cell(ParticipantsSheetColumns.Note).SetValue(participant.Note)
                        .SetDataType(XLDataType.Text);
                    row.Cell(ParticipantsSheetColumns.SignedBy).SetValue(participant.SignedBy)
                        .SetDataType(XLDataType.Text);
                    if (participant.SignedAtUtc.HasValue)
                    {
                        AddDateCell(row, ParticipantsSheetColumns.SignedAtUtc, participant.SignedAtUtc.Value, false);
                    }
                }

                rowIdx++;
                row.InsertRowsBelow(1);
            }

            const int minWidth = 10;
            const int maxWidth = 100;
            severalParticipantsSheet.Columns(1, ParticipantsSheetColumns.Last).AdjustToContents(1, rowIdx, minWidth, maxWidth);
        }

        private void CreateInvitationSheet(XLWorkbook workbook, IEnumerable<ExportInvitationDto> invitations)
        {
            var sheet = workbook.Worksheets.Add("Invitations");

            var rowIdx = 0;
            var row = sheet.Row(++rowIdx);
            row.Style.Font.SetBold();
            row.Style.Font.SetFontSize(12);
            row.Cell(InvitationSheetColumns.IpoNo).Value = "Ipo no";
            row.Cell(InvitationSheetColumns.Status).Value = "Status";
            row.Cell(InvitationSheetColumns.Title).Value = "Title";
            row.Cell(InvitationSheetColumns.Description).Value = "Description";
            row.Cell(InvitationSheetColumns.Location).Value = "Location";
            row.Cell(InvitationSheetColumns.Type).Value = "Type";
            row.Cell(InvitationSheetColumns.StartTimeUtc).Value = "Start time";
            row.Cell(InvitationSheetColumns.EndTimeUtc).Value = "End time";
            row.Cell(InvitationSheetColumns.McPkgs).Value = "Mc pkgs";
            row.Cell(InvitationSheetColumns.CommPkgs).Value = "Comm pkgs";
            row.Cell(InvitationSheetColumns.ContractorRep).Value = "Contractor rep";
            row.Cell(InvitationSheetColumns.ConstructionCompanyRep).Value = "Construction company rep";
            row.Cell(InvitationSheetColumns.CompletedAtUtc).Value = "Completed at";
            row.Cell(InvitationSheetColumns.AcceptedAtUtc).Value = "Accepted at";
            row.Cell(InvitationSheetColumns.CreatedBy).Value = "Created by";
            row.Cell(InvitationSheetColumns.CreatedAtUtc).Value = "Created at";

            foreach (var invitation in invitations)
            {
                row = sheet.Row(++rowIdx);

                row.Cell(InvitationSheetColumns.IpoNo).SetValue(invitation.Id).SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.Status).SetValue(invitation.Status).SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.Title).SetValue(invitation.Title).SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.Description).SetValue(invitation.Description).SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.Location).SetValue(invitation.Location).SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.Type).SetValue(invitation.Type).SetDataType(XLDataType.Text);
                AddDateCell(row, InvitationSheetColumns.StartTimeUtc, invitation.StartTimeUtc, false);
                AddDateCell(row, InvitationSheetColumns.EndTimeUtc, invitation.EndTimeUtc, false);
                row.Cell(InvitationSheetColumns.McPkgs)
                    .SetValue(string.Join(", ", invitation.McPkgs))
                    .SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.CommPkgs)
                    .SetValue(string.Join(", ", invitation.CommPkgs))
                    .SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.ContractorRep).SetValue(invitation.ContractorRep).SetDataType(XLDataType.Text);
                row.Cell(InvitationSheetColumns.ConstructionCompanyRep).SetValue(invitation.ConstructionCompanyRep).SetDataType(XLDataType.Text);
                if (invitation.CompletedAtUtc.HasValue)
                {
                    AddDateCell(row, InvitationSheetColumns.CompletedAtUtc, invitation.CompletedAtUtc.Value.Date);
                }
                if (invitation.AcceptedAtUtc.HasValue)
                {
                    AddDateCell(row, InvitationSheetColumns.AcceptedAtUtc, invitation.AcceptedAtUtc.Value.Date);
                }
                row.Cell(InvitationSheetColumns.CreatedBy).SetValue(invitation.CreatedBy).SetDataType(XLDataType.Text);
                AddDateCell(row, InvitationSheetColumns.CreatedAtUtc, invitation.CreatedAtUtc, false);
            }

            const int minWidth = 10;
            const int maxWidth = 100;
            sheet.Columns(1, InvitationSheetColumns.Last).AdjustToContents(1, rowIdx, minWidth, maxWidth);
        }

        private void CreateFrontSheet(XLWorkbook workbook, UsedFilterDto usedFilter)
        {
            var sheet = workbook.Worksheets.Add("Filters");
            var row = sheet.Row(FrontSheetRows.MainHeading);
            row.Style.Font.SetBold();
            row.Style.Font.SetFontSize(14);
            row.Cell(1).Value = "Export of invitation to punch-outs";

            AddUsedFilter(sheet.Row(FrontSheetRows.Plant), "Plant", usedFilter.Plant, true);
            AddUsedFilter(sheet.Row(FrontSheetRows.ProjectName), "Project name", usedFilter.ProjectName, true);

            AddUsedFilter(sheet.Row(FrontSheetRows.FilterHeading), "Filter values:", "", true);

            AddUsedFilter(sheet.Row(FrontSheetRows.Invitation), "Ipo number starts with", usedFilter.IpoIdStartsWith);
            AddUsedFilter(sheet.Row(FrontSheetRows.Title), "Title starts with", usedFilter.IpoTitleStartWith);
            AddUsedFilter(sheet.Row(FrontSheetRows.CommPkg), "CommPkg number starts with", usedFilter.CommPkgNoStartWith);
            AddUsedFilter(sheet.Row(FrontSheetRows.McPkg), "McPkg number starts with", usedFilter.McPkgNoStartsWith);
            AddUsedFilter(sheet.Row(FrontSheetRows.PunchOutDateFrom), "Punch out dates from", usedFilter.PunchOutDateFromUtc);
            AddUsedFilter(sheet.Row(FrontSheetRows.PunchOutDateTo), "Punch out dates to", usedFilter.PunchOutDateToUtc);
            AddUsedFilter(sheet.Row(FrontSheetRows.LastChangedFrom), "Last changed dates from", usedFilter.LastChangedFromUtc);
            AddUsedFilter(sheet.Row(FrontSheetRows.LastChangedTo), "Last changed dates to", usedFilter.LastChangedToUtc);
            AddUsedFilter(sheet.Row(FrontSheetRows.Status), "Ipo status", usedFilter.IpoStatuses);
            AddUsedFilter(sheet.Row(FrontSheetRows.Role), "Functional role invited", usedFilter.FunctionalRoleInvited);
            AddUsedFilter(sheet.Row(FrontSheetRows.Person), "Person invited", usedFilter.PersonInvited);

            sheet.Columns(1, 2).AdjustToContents();
        }

        private void AddUsedFilter(IXLRow row, string label, IEnumerable<string> values)
            => AddUsedFilter(row, label, string.Join(",", values));

        private void AddUsedFilter(IXLRow row, string label, string value, bool bold = false)
        {
            row.Cell(1).SetValue(label).SetDataType(XLDataType.Text);
            row.Cell(2).SetValue(value).SetDataType(XLDataType.Text);
            row.Style.Font.SetBold(bold);
        }

        private void AddUsedFilter(IXLRow row, string label, DateTime? date, bool bold = false)
        {
            row.Cell(1).SetValue(label).SetDataType(XLDataType.Text);
            if (date.HasValue)
            {
                AddDateCell(row, 2, date.Value.Date);
            }

            row.Style.Font.SetBold(bold);
        }

        public string GetFileName() => $"InvitationForPunchOuts-{DateTime.Now:yyyyMMdd-hhmmss}";
    }
}
