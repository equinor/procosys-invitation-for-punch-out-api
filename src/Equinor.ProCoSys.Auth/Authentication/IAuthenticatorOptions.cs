using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Auth.Authentication
{
    public interface IAuthenticatorOptions
    {
        string Instance { get; }
        string ClientId { get; }
        string Secret { get; }
        Guid ObjectId { get; }
        
        IDictionary<string, string> Scopes { get; }
    }
}
