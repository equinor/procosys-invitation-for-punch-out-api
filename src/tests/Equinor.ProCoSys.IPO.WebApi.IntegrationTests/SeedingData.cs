using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public class SeedingData
    {
        public static string Plant => "PCS$PLANT1";
        public static string ProjectName => "TestProject";
        public static string ProjectDescription => "Test - Project";
        public static string Invitation => "TestInvitation";
        public static string InvitationDescription => "Test - Invitation";
        public static Guid MeetingId => new Guid("{818E6882-A5F1-4367-B459-1A2E1EE01D7F}");

        public List<int> InvitationIds = new List<int>();
    }
}
