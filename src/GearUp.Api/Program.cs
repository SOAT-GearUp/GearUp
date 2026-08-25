using GearUp.Api.ErrorHandling;
using GearUp.Api.HealthChecks;
using GearUp.Application;
using GearUp.Infrastructure;
using GearUp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            []
        }
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key não configurada.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key deve possuir pelo menos 32 bytes.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Health checks consumidos pelas probes do Kubernetes (ver k8s/deployment.yaml).
builder.Services.AddHealthChecks()
    .AddCheck<BancoDeDadosHealthCheck>(
        BancoDeDadosHealthCheck.Nome,
        tags: [BancoDeDadosHealthCheck.TagProntidao]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnablePersistAuthorization();
        options.InjectJavascript("/swagger/swagger-auth.js");
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// livenessProbe/startupProbe: responde 200 se o processo está de pé. Não checa
// dependências externas — reiniciar o pod não conserta um banco fora do ar.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();

// readinessProbe: só entra no balanceamento do Service quando o PostgreSQL
// responde.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registro => registro.Tags.Contains(BancoDeDadosHealthCheck.TagProntidao)
}).AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
}

await app.RunAsync();

public partial class Program { }
