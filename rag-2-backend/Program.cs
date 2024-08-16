using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using rag_2_backend.data;
using rag_2_backend.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using rag_2_backend.Utils;
using rag_2_backend.Services;

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
                {
                    throw new UnauthorizedAccessException("Token is not valid");
                }
            }
        };
    });

//Jwt configuration

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
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
    options.AddDefaultPolicy(builder =>
        builder.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameRecordService>();
builder.Services.AddScoped<JwtUtil>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<EmailSendingUtil>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<BackgroundServiceImpl>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();