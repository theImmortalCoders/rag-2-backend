using System.Security.Claims;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.Utils;

namespace rag_2_backend.Services;

public class UserService(DatabaseContext context, JwtUtil jwtUtil)
{
    public void RegisterUser(UserRequest userRequest)
    {

    }

    public string GetMe()
    {
        return "";
    }

    public string LoginUser(string username, string password)
    {
        return jwtUtil.GenerateToken("marcinbator.ofc@gmail.com", "admin");
    }

    public void LogoutUser()
    {
        //logout logic
    }
}

