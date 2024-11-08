#region

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Config;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Database;
using StackExchange.Redis;

#endregion

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => { b.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null); });
});
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:ConnectionString").Value ?? "")
);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        {
            AuthConfig.ConfigureJwt(
                options,
                builder.Configuration.GetSection("Jwt:Issuer").Get<string>(),
                builder.Configuration.GetSection("Jwt:Key").Get<string>());
        }
    );
builder.Services.RegisterServices(builder.Configuration);
AuthConfig.ConfigureCors(builder.Services, builder.Configuration);

var app = builder.Build();
Console.WriteLine(app.Environment.IsDevelopment() ? "Development" : "Production");
DbConfig.MigrateDb(app);

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();