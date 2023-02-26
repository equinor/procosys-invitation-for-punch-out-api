using System.Collections.Generic;

namespace Equinor.ProCoSys.Auth.Authentication
{
    /// <summary>
    /// Loop through all classes implemeting the IBearerTokenSetter interface and
    /// set given bearerToken
    /// </summary>
    public class BearerTokenSetterForAll : IBearerTokenSetterForAll
    {
        private readonly IEnumerable<IBearerTokenSetter> _bearerTokenSetters;

        public BearerTokenSetterForAll(IEnumerable<IBearerTokenSetter> bearerTokenSetters)
            => _bearerTokenSetters = bearerTokenSetters;

        public void SetBearerToken(string bearerToken)
        {
            foreach (var bearerTokenSetter in _bearerTokenSetters)
            {
                bearerTokenSetter.SetBearerToken(bearerToken);
            }
        }
    }
}
