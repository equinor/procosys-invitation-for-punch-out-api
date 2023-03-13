using System;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization
{
    [Obsolete("Revert to use Topic-classes from Equinor.ProCoSys.PcsServiceBus after ensuring real Guids on bus.")]
    public class LibraryTmpTopic
    {
        public string Plant
        {
            get;
            set;
        }

        public string ProCoSysGuid
        {
            get;
            set;
        }

        public string Behavior
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        public string CodeOld
        {
            get;
            set;
        }

        public string LibraryId
        {
            get;
            set;
        }

        public string ParentId
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public bool IsVoided
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string LastUpdated
        {
            get;
            set;
        }
    }
}
