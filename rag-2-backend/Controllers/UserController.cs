using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IConfiguration config)
{
    private IConfiguration _config = config;

    [HttpPost("login")]
    public ActionResult<string> Post([FromBody] LoginRequest loginRequest)
    {
        //your logic for login process
        //If login usrename and password are correct then proceed to generate token

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
          _config["Jwt:Issuer"],
          null,
          expires: DateTime.Now.AddMinutes(120),
          signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

        return token;
    }
}