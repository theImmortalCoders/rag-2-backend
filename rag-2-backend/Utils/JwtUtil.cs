using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rag_2_backend.data;

namespace rag_2_backend.Utils;

public class JwtUtil(IConfiguration config, DatabaseContext context)

{
    public virtual string GenerateToken(string email, string role)
    {
        var jwtKey = config["Jwt:Key"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        ];

        var token = new JwtSecurityToken(
            config["Jwt:Issuer"],
            config["Jwt:Issuer"],
            claims,
            expires: DateTime.Now.AddMinutes(double.Parse(config["Jwt:ExpireMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return await context.BlacklistedJwts.AnyAsync(bt => bt.Token == token);
    }
}