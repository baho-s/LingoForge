using BeeZillion.API;
using BeeZillion.API.Middleware;
using BeeZillion.Application;
using BeeZillion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS POLİTİKASINI BURAYA EKLİYORUZ (builder.Build() satırından önce)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel",
        policy =>
        {
            policy.WithOrigins(
                "https://lingo-forge-iota.vercel.app", // Canlı linkin dursun
                "http://localhost:5173"                   // VİRGÜL KOYUP BUNU EKLE (Vite'ın varsayılan portu)
            ) 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Auth kullanıyorsan
        });
});

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.UseCors("AllowVercel");
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

