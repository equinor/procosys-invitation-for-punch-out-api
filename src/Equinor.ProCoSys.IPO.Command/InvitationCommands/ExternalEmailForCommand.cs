﻿namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class ExternalEmailForCommand
    {
        public ExternalEmailForCommand(
            string email,
            int? id = null,
            string rowVersion = null)
        {
            Email = email;
            Id = id;
            RowVersion = rowVersion;
        }
        public string Email { get; }
        public int? Id { get; }
        public string RowVersion { get; }
    }
}
