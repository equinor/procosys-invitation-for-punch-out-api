﻿using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class ExternalEmailDto
    {
        public ExternalEmailDto(
            int id,
            string externalEmail)
        {
            Id = id;
            ExternalEmail = externalEmail;
        }

        public int Id { get; }
        public string ExternalEmail { get; }
        public OutlookResponse? Response { get; set; }
    }
}
