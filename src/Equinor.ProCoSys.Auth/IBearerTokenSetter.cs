﻿namespace Equinor.ProCoSys.Auth
{
    public interface IBearerTokenSetter
    {
        void SetBearerToken(string token, bool isUserToken = true);
    }
}
