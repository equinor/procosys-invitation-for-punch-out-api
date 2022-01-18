﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Behaviors
{
    public class CheckAccessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<CheckAccessBehavior<TRequest, TResponse>> _logger;
        private readonly IAccessValidator _accessValidator;
        public CheckAccessBehavior(ILogger<CheckAccessBehavior<TRequest, TResponse>> logger, IAccessValidator accessValidator)
        {
            _logger = logger;
            _accessValidator = accessValidator;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var typeName = request.GetGenericTypeName();

            _logger.LogInformation($"----- Checking access for {typeName}");

            if (!await _accessValidator.ValidateAsync(request as IBaseRequest))
            {
                _logger.LogWarning($"User do not have access - {typeName}");

                throw new UnauthorizedAccessException();
            }

            return await next();
        }
    }
}
