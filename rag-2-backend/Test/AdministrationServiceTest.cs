using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using rag_2_backend.Config;
using rag_2_backend.DTO;
using rag_2_backend.DTO.User;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;
using rag_2_backend.Services;
using rag_2_backend.Utils;
using Xunit;

namespace rag_2_backend.Test;

public class AdministrationServiceTest
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly Mock<UserUtil> _userMock;

    private readonly AdministrationService _administrationService;

    private readonly User _admin = new("email@prz.edu.pl")
    {
        Id = 1,
        Name = "John",
        Password = "password",
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023,
        Role = Role.Admin
    };

    private readonly User _user = new("email2@stud.prz.edu.pl")
    {
        Id = 2,
        Name = "John",
        Password = "password",
        StudyCycleYearA = 2022,
        StudyCycleYearB = 2023
    };

    public AdministrationServiceTest()
    {
        _contextMock.Setup(c => c.Users).Returns(
            new List<User> { _admin, _user }.AsQueryable().BuildMockDbSet().Object
        );
        _userMock = new Mock<UserUtil>(_contextMock.Object);
        _userMock.Setup(u => u.GetUserByIdOrThrow(It.IsAny<int>())).Returns(_user);
        _userMock.Setup(u => u.GetUserByEmailOrThrow(It.IsAny<string>())).Returns(_admin);
        _administrationService = new AdministrationService(_contextMock.Object, _userMock.Object);
    }

    [Fact]
    public void ShouldBanUser()
    {
        _administrationService.ChangeBanStatus(2, true);

        Assert.True(_user.Banned);

        _userMock.Setup(u => u.GetUserByIdOrThrow(It.IsAny<int>())).Returns(_admin);
        Assert.Throws<BadHttpRequestException>(
            () => _administrationService.ChangeBanStatus(1, false));
    }

    [Fact]
    public void ShouldChangeRole()
    {
        _administrationService.ChangeRole(2, Role.Student);

        Assert.Equal(Role.Student, _user.Role);

        _userMock.Setup(u => u.GetUserByIdOrThrow(It.IsAny<int>())).Returns(_admin);
        Assert.Throws<BadHttpRequestException>(
            () => _administrationService.ChangeRole(1, Role.Teacher));
    }

    [Fact]
    public void ShouldGetUserDetails()
    {
        var response = new UserResponse
        {
            Id = 2,
            Email = "email2@stud.prz.edu.pl",
            Name = "John",
            Role = Role.Student,
            StudyCycleYearA = 2022,
            StudyCycleYearB = 2023
        };

        Assert.Equal(JsonConvert.SerializeObject(response),
            JsonConvert.SerializeObject(_administrationService.GetUserDetails("email@prz.edu.pl", 1)));
        
        _userMock.Setup(u => u.GetUserByEmailOrThrow(It.IsAny<string>())).Returns(_user);
        Assert.Throws<KeyNotFoundException>(() => _administrationService.GetUserDetails("email1@prz.edu.pl", 1));
    }

    [Fact]
    public void ShouldGetStudents()
    {
        var response = new UserResponse
        {
            Id = 2,
            Email = "email2@stud.prz.edu.pl",
            Name = "John",
            Role = Role.Student,
            StudyCycleYearA = 2022,
            StudyCycleYearB = 2023
        };

        Assert.Equal(
            JsonConvert.SerializeObject(new List<UserResponse> { response }),
            JsonConvert.SerializeObject(_administrationService.GetStudents(2022, 2023))
        );
    }
}