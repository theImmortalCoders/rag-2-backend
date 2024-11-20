#region

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Module.Administration;
using rag_2_backend.Infrastructure.Module.Auth;
using rag_2_backend.Infrastructure.Module.Background;
using rag_2_backend.Infrastructure.Module.Course;
using rag_2_backend.Infrastructure.Module.Email;
using rag_2_backend.Infrastructure.Module.Game;
using rag_2_backend.Infrastructure.Module.GameRecord;
using rag_2_backend.Infrastructure.Module.Stats;
using rag_2_backend.Infrastructure.Module.User;
using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Config;

public static class ServiceRegistrationExtension
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        ConfigServices(services);
        ConfigSwagger(services);
    }

    //

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
        services.AddScoped<AuthService>();
        services.AddScoped<StatsService>();
        services.AddScoped<UserDao>();
        services.AddScoped<RefreshTokenDao>();
        services.AddScoped<GameDao>();
        services.AddScoped<GameRecordDao>();
        services.AddScoped<StatsUtil>();
        services.AddScoped<CourseDao>();
        services.AddScoped<CourseService>();
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