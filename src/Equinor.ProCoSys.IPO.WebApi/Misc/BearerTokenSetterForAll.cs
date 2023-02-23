using System.Collections.Generic;
using Equinor.ProCoSys.Auth;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
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
