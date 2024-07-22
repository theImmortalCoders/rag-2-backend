using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Utils;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(JwtUtil jwtUtil) : ControllerBase
{

	[HttpPost]
	public ActionResult<string> Post([FromBody] LoginRequest loginRequest)
	{
		//login logic

		return jwtUtil.GenerateToken("marcin", "admin");
	}

	[HttpPost("logout")]
	public void Logout()
	{
		Console.WriteLine("xd");
	}
}