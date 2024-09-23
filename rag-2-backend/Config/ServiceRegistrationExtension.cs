#region

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using rag_2_backend.Services;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.Config;

public static class ServiceRegistrationExtension
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(b =>
                b.WithOrigins(configuration.GetValue<string>("AllowedOrigins") ?? string.Empty)
                    .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
        });
        services.AddEndpointsApiExplorer();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        ConfigServices(services);
        ConfigSwagger(services);

        return services;
    }

    private static void ConfigServices(IServiceCollection services)
    {
        services.AddHostedService<BackgroundServiceImpl>();
        services.AddScoped<UserService>();
        services.AddScoped<GameRecordService>();
        services.AddScoped<JwtUtil>();
        services.AddScoped<GameService>();
        services.AddScoped<EmailSendingUtil>();
        services.AddScoped<EmailService>();
        services.AddScoped<JwtSecurityTokenHandler>();
        services.AddScoped<AdministrationService>();
        services.AddScoped<StatsService>();
        services.AddScoped<UserUtil>();
    }

    private static void ConfigSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(s =>
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "rag-2-backend.xml");
            s.IncludeXmlComments(filePath);
        });
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}