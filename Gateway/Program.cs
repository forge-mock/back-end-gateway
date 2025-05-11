using DotNetEnv;
using Gateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shared.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Env.Load("../.env");
string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? string.Empty;

builder.Configuration.AddOcelot();
builder.Services.AddCors(options =>
    options.AddPolicy(
        "Development",
        policy =>
        {
            policy.WithOrigins("https://localhost:3000")
                .AllowCredentials()
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .WithHeaders("Authorization", "Content-Type");
        }));
builder.Services.AddOcelot();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddApiServices();

WebApplication app = builder.Build();

app.Use(async (context, next) =>
{
    IMiddlewareService middlewareService = app.Services.GetRequiredService<IMiddlewareService>();
    middlewareService.ConfigureHeaders(ref context);
    await next();
});

app.UseCors("Development");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseOcelot().Wait();

await app.RunAsync();