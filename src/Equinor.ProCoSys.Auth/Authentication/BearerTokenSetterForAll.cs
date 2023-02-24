using System.Collections.Generic;

namespace Equinor.ProCoSys.Auth.Authentication
{
    public class BearerTokenSetterForAll : IBearerTokenSetterForAll
    {
        private readonly IEnumerable<IBearerTokenSetter> _bearerTokenSetters;

        public BearerTokenSetterForAll(IEnumerable<IBearerTokenSetter> bearerTokenSetters)
            => _bearerTokenSetters = bearerTokenSetters;

        public void SetBearerToken(string token)
        {
            foreach (var bearerTokenSetter in _bearerTokenSetters)
            {
                bearerTokenSetter.SetBearerToken(token);
            }
        }
    }
}
