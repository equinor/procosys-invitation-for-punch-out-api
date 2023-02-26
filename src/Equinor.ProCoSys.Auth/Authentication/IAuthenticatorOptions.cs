using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Auth.Authentication
{
    /// <summary>
    /// Interface to be implemented to map API specific configurations to this generic configuration
    /// Used in ApiAuthenticator when creating a bearer token for access a foreign API
    /// </summary>
    public interface IAuthenticatorOptions
    {
        string Instance { get; }
        string ClientId { get; }
        string Secret { get; }
        Guid ObjectId { get; }

        /// <summary>
        /// set true to NOT add UserData claims for Projects in ClaimsTransformation
        /// </summary>
        bool DisableProjectUserDataClaims { get; }

        /// <summary>
        /// set true to NOT add UserData claims for Restriction Roles in ClaimsTransformation
        /// </summary>
        bool DisableRestrictionRoleUserDataClaims { get; }

        /// <summary>
        /// Key/value list of scopes to be used when creating bearer token in an ApiAuthenticator implementation.
        /// The ApiAuthenticator constructor must be called with the correct ApiScopeKey
        /// referring to the correct scope in list of Scopes. See MainApiAuthenticator for sample impl
        /// </summary>
        IDictionary<string, string> Scopes { get; }
    }
}
