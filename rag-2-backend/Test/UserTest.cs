#region

using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Database.Entity;
using Xunit;

#endregion

namespace rag_2_backend.Test;

public class UserTest
{
    [Fact]
    public void ShouldCreateUser()
    {
        Assert.Equal(
            Role.Student,
            new User("index@stud.prz.edu.pl")
                { Password = "pass", StudyCycleYearA = 2022, StudyCycleYearB = 2023, Name = "John" }.Role
        );
        Assert.Equal(
            Role.Teacher,
            new User("index@prz.edu.pl")
                { Password = "pass", StudyCycleYearA = 2022, StudyCycleYearB = 2023, Name = "John" }.Role
        );
        Assert.Throws<BadHttpRequestException>(
            () => new User("index@gmail.com")
                { Password = "pass", StudyCycleYearA = 2022, StudyCycleYearB = 2023, Name = "John" }
        );
    }
}