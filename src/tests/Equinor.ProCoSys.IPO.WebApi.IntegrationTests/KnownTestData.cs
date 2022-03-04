﻿using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public class KnownTestData
    {
        public static string Plant => "PCS$PLANT1";
        public static string ProjectName => "TestProject";
        public static string ProjectDescription => "Test - Project";
        public static string McPkgNo => "MC10-2034";
        public static string CommPkgNo => "COMM12-2387";
        public static string System => "1|2";
        public static string FunctionalRoleCode => "FRC";
        public static string InvitationTitle => "TestInvitation";
        public static string InvitationDescription => "Test - Invitation";
        public static Guid MeetingId => new Guid("{818E6882-A5F1-4367-B459-1A2E1EE01D7F}");

        public List<int> MdpInvitationIds = new List<int>();
        public List<int> DpInvitationIds = new List<int>();
        public List<int> CommentIds = new List<int>();
        public List<int> McPkgIds = new List<int>();
        public List<int> CommPkgIds = new List<int>();
    }
}
