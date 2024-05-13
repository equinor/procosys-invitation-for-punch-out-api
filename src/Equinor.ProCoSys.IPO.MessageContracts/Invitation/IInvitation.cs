﻿namespace Equinor.ProCoSys.IPO.MessageContracts.Invitation;

public interface IInvitation
{
    public Guid ProCoSysGuid { get;  }
    public string Plant { get;  }
    public string ProjectName { get;  }
    public string IpoNumber { get;  }
    public DateTime CreatedAtUtc { get;  }
    public Guid CreatedByOid { get;  }
    public DateTime? ModifiedAtUtc { get;  }
    public string Title { get;  }
    public string Type { get;  }
    public string Description { get;  }
    public string Status { get;  }
    public DateTime EndTimeUtc { get;  }
    public string Location { get;  }
    public DateTime StartTimeUtc { get;  }
    public DateTime? AcceptedAtUtc { get;  }
    public Guid AcceptedByOid { get;  }
    public DateTime? CompletedAtUtc { get;  }
    public Guid CompletedByOid { get;  }

}
