using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using rag_2_backend.Config;
using rag_2_backend.Models;
using rag_2_backend.Services;
using rag_2_backend.Utils;

var builder = WebApplication.CreateBuilder(args);

//Jwt configuration
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = async context =>
            {
                var tokenBlacklistService = context.HttpContext.RequestServices.GetRequiredService<JwtUtil>();
                var header = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
                if (header == null) return;
                var token = header["Bearer ".Length..].Trim();

                if (!string.IsNullOrEmpty(token) && await tokenBlacklistService.IsTokenBlacklistedAsync(token))
                    throw new UnauthorizedAccessException("Token is not valid");
            }
        };
    });

//Jwt configuration

builder.Services.AddSwaggerGen(options =>
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSwaggerGen(s =>
{
    var filePath = Path.Combine(AppContext.BaseDirectory, "rag-2-backend.xml");
    s.IncludeXmlComments(filePath);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(b =>
        b.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHostedService<BackgroundServiceImpl>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameRecordService>();
builder.Services.AddScoped<JwtUtil>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<EmailSendingUtil>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JwtSecurityTokenHandler>();
builder.Services.AddScoped<AdministrationService>();

var app = builder.Build();

Console.WriteLine(app.Environment.IsDevelopment() ? "Development" : "Production");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DatabaseContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
        throw;
    }
}

app.UseCors();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();