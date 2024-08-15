using rag_2_backend.Models;
using rag_2_backend.Models.Entity;
using Xunit;

namespace rag_2_backend.Test;

public class UserTest
{
    [Fact]
    public void ShouldCreateUser()
    {
        Assert.Equal(
            Role.Student,
            new User("index@stud.prz.edu.pl"){Password = "pass"}.Role
        );
        Assert.Equal(
            Role.Teacher,
            new User("index@prz.edu.pl"){Password = "pass"}.Role
        );
        Assert.Throws<BadHttpRequestException>(
            () => new User("index@gmail.com"){Password = "pass"}
        );
    }
}