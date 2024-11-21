#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Course;
using rag_2_backend.Infrastructure.Module.Course.Dto;
using Xunit;

#endregion

namespace rag_2_backend.Test.Service;

public class CourseServiceTest
{
    private readonly Mock<DatabaseContext> _contextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    private readonly List<Course> _courses =
    [
        new() { Id = 1, Name = "Course1" },
        new() { Id = 2, Name = "Course2" }
    ];

    private readonly CourseService _courseService;

    public CourseServiceTest()
    {
        Mock<CourseDao> courseDaoMock = new(_contextMock.Object);
        _courseService = new CourseService(_contextMock.Object, courseDaoMock.Object);
        courseDaoMock.Setup(dao => dao.GetCourseByIdOrThrow(It.IsAny<int>())).ReturnsAsync(_courses[0]);
        _contextMock.Setup(c => c.Courses).Returns(_courses.AsQueryable().BuildMockDbSet().Object);
    }

    [Fact]
    public async Task ShouldGetAllCourses()
    {
        var actualCourses = await _courseService.GetCourses();

        Assert.Equal(JsonConvert.SerializeObject(_courses.Select(CourseMapper.Map)),
            JsonConvert.SerializeObject(actualCourses));
    }

    [Fact]
    public async Task ShouldAddCourse()
    {
        var courseRequest = new CourseRequest
        {
            Name = "Course3"
        };

        await _courseService.AddCourse(courseRequest);

        _contextMock.Verify(
            c => c.Courses.Add(It.Is<Course>(course => course.Name == courseRequest.Name)),
            Times.Once);
    }

    [Fact]
    public async Task ShouldNotAddCourseIfCourseAlreadyExists()
    {
        var courseRequest = new CourseRequest
        {
            Name = "Course1"
        };

        await Assert.ThrowsAsync<BadRequestException>(() => _courseService.AddCourse(courseRequest));
    }

    [Fact]
    public async Task ShouldRemoveCourse()
    {
        var users = new List<User>
        {
            new()
            {
                Id = 1,
                Name = "User1",
                Password = null!,
                Course = _courses[1]
            }
        };

        _contextMock.Setup(c => c.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        await _courseService.RemoveCourse(1);

        _contextMock.Verify(c => c.Courses.Remove(It.Is<Course>(course => course.Id == 1)), Times.Once);
    }

    [Fact]
    public async Task ShouldThrowWhenRemoveCourse()
    {
        var users = new List<User>
        {
            new()
            {
                Id = 1,
                Name = "User1",
                Password = null!,
                Course = _courses[0]
            }
        };
        _contextMock.Setup(c => c.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);

        await Assert.ThrowsAsync<BadRequestException>(() => _courseService.RemoveCourse(1));
    }

    [Fact]
    public async Task ShouldUpdateCourse()
    {
        var courseRequest = new CourseRequest
        {
            Name = "Course3"
        };

        await _courseService.EditCourse(courseRequest, 1);

        _contextMock.Verify(
            c => c.Courses.Update(It.Is<Course>(course => course.Name == courseRequest.Name)),
            Times.Once);
    }

    [Fact]
    public async Task ShouldNotUpdateCourseIfCourseAlreadyExists()
    {
        var courseRequest = new CourseRequest
        {
            Name = "Course2"
        };

        await Assert.ThrowsAsync<BadRequestException>(() => _courseService.EditCourse(courseRequest, 1));
    }
}