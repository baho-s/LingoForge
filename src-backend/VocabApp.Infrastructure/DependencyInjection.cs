using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.Text;
using VocabApp.Application.Common.Interfaces;
using VocabApp.Domain.Repositories;
using VocabApp.Infrastructure.AI;
using VocabApp.Infrastructure.Auth;
using VocabApp.Infrastructure.Cache;
using VocabApp.Infrastructure.Events;
using VocabApp.Infrastructure.Persistence;
using VocabApp.Infrastructure.Persistence.Interceptors;
using VocabApp.Infrastructure.Persistence.Repositories;

namespace VocabApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        services.Configure<JwtOptions>(jwtSection);

        var groqSection = configuration.GetSection("Groq");
        services.Configure<GroqOptions>(groqSection);

        var jwtOptions = jwtSection.Get<JwtOptions>();
        if (jwtOptions is null || string.IsNullOrWhiteSpace(jwtOptions.Secret))
        {
            throw new InvalidOperationException("Jwt configuration is missing or invalid.");
        }

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddHttpClient<IAiSentenceService, GroqService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GroqOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(20);
        })
        .AddPolicyHandler(GetAiRetryPolicy());

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is missing.");
        }

        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseSqlServer(connectionString)
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>()));

        services.AddMemoryCache();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<DomainEventDispatcherInterceptor>();
        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetAiRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt))
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)));
    }
}
