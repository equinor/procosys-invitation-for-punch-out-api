﻿using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Equinor.Procosys.CPO.WebApi.Misc
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly ClaimsPrincipal principal;

        public ClaimsProvider(IHttpContextAccessor accessor) => principal = accessor?.HttpContext?.User ?? new ClaimsPrincipal();

        public ClaimsPrincipal GetCurrentUser() => principal;
    }
}
