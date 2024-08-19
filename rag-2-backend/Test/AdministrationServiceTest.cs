using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using rag_2_backend.data;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;
using rag_2_backend.Services;
using Xunit;

namespace rag_2_backend.Test;

public class AdministrationServiceTest
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly AdministrationService _administrationService;

    private readonly User _user = new("email@prz.edu.pl")
    {
        Id = 1,
        Name = "John",
        Password = "password",
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023
    };

    public AdministrationServiceTest()
    {
        _contextMock.Setup(c => c.Users).Returns(
            new List<User> { _user }.AsQueryable().BuildMockDbSet().Object
        );
        _administrationService = new AdministrationService(_contextMock.Object);
    }

    [Fact]
    public void ShouldBanUser()
    {
        _administrationService.ChangeBanStatus(1, true);

        Assert.True(_user.Banned);

        _user.Role = Role.Admin;
        Assert.Throws<BadHttpRequestException>(
            ()=>_administrationService.ChangeBanStatus(1, false));
    }

    [Fact]
    public void ShouldChangeRole()
    {
        _administrationService.ChangeRole(1, Role.Student);

        Assert.Equal(Role.Student, _user.Role);

        _user.Role = Role.Admin;
        Assert.Throws<BadHttpRequestException>(
            ()=>_administrationService.ChangeRole(1, Role.Teacher));
    }
}