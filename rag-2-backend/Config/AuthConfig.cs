#region

using System.Text;
using HttpExceptions.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

#endregion

namespace rag_2_backend.Config;

public static class AuthConfig
{
    public static void ConfigureJwt(JwtBearerOptions jwtBearerOptions, string? s, string? jwtKey1)
    {
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = s,
            ValidAudience = s,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey1 ?? ""))
        };

        jwtBearerOptions.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var header = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
                if (header == null) return Task.CompletedTask;
                var token = header["Bearer ".Length..].Trim();

                if (string.IsNullOrEmpty(token))
                    throw new UnauthorizedException("Token is not valid");

                return Task.CompletedTask;
            }
        };
    }

    public static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetValue<string>("AllowedOrigins")?.Split(',');

            options.AddPolicy("AllowSpecificOrigins", builder =>
            {
                builder.WithOrigins(allowedOrigins ?? [])
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        });
    }
}