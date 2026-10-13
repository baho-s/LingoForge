using BeeZillion.API;
using BeeZillion.API.Middleware;
using BeeZillion.Application;
using BeeZillion.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS POLİTİKASINI BURAYA EKLİYORUZ (builder.Build() satırından önce)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel",
        policy =>
        {
            policy.SetIsOriginAllowed(origin =>
                      origin is "http://localhost:5173" or "http://localhost:5174"
                      || (Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                          && uri.Scheme == Uri.UriSchemeHttps
                          && uri.Host.StartsWith("beezillion", StringComparison.OrdinalIgnoreCase)
                          && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)))
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Auth kullanıyorsan
        });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Hosting proxy'sinin IP'si bilinmediği için güvenilir ağ listeleri temizlenir
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.UseCors("AllowVercel");
app.UseRateLimiter();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


// DevController'ı production'dan exclude et
#if !DEBUG
app.MapGet("/api/dev/{**route}", () => Results.NotFound())
    .WithName("DevNotFoundProduction");
#endif

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

