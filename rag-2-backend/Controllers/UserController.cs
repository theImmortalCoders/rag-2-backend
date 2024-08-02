using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.DTO;
using rag_2_backend.Services;

namespace rag_2_backend.controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
	[HttpPost("auth/register")]
	public void Post([FromBody] UserRequest userRequest)
	{
		userService.RegisterUser(userRequest);
	}

	[HttpPost("auth/login")]
	public ActionResult<string> Post([FromBody] LoginRequest loginRequest)
	{
		return userService.LoginUser(loginRequest.Email, loginRequest.Password);
	}

	[HttpPost("auth/logout")]
	public void Logout()
	{
		var email = (User.FindFirst(ClaimTypes.Email)?.Value) ?? throw new UnauthorizedAccessException("Unauthorized");

		userService.LogoutUser(email);
	}

	[HttpGet("me")]
	[Authorize]
	public UserResponse Me()
	{
		var email = (User.FindFirst(ClaimTypes.Email)?.Value) ?? throw new UnauthorizedAccessException("Unauthorized");

		return userService.GetMe(email);
	}
}