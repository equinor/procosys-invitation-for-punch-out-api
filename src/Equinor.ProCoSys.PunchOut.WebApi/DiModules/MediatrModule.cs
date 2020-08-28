using System.Reflection;
using Equinor.ProCoSys.PunchOut.Command;
using Equinor.ProCoSys.PunchOut.Query;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.PunchOut.WebApi.DiModules
{
    public static class MediatrModule
    {
        public static void AddMediatrModules(this IServiceCollection services)
        {
            services.AddMediatR(
                typeof(MediatrModule).GetTypeInfo().Assembly,
                typeof(ICommandMarker).GetTypeInfo().Assembly,
                typeof(IQueryMarker).GetTypeInfo().Assembly
            );
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CheckAccessBehavior<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
        }
    }
}
