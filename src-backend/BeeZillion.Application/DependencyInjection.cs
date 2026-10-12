using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using BeeZillion.Application.Common.Behaviors;
using BeeZillion.Application.Common.Interfaces;
using BeeZillion.Application.Common.Services;

namespace BeeZillion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        services.AddScoped<IAiUsageLimiter, AiUsageLimiter>();

        return services;
    }
}

