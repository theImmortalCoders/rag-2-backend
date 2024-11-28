#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using Xunit;

#endregion

namespace rag_2_backend.Test.Dao;

public class CourseDaoTest
{
    private readonly CourseDao _courseDao;

    private readonly Mock<DatabaseContext> _dbContextMock = new(
        new DbContextOptionsBuilder<DatabaseContext>().Options
    );

    public CourseDaoTest()
    {
        _courseDao = new CourseDao(_dbContextMock.Object);
    }

    private void SetUpCourses(IEnumerable<Course> courses)
    {
        _dbContextMock.Setup(db => db.Courses).ReturnsDbSet(courses);
    }

    [Fact]
    public async Task GetCourseById_ShouldReturnCourse()
    {
        var expectedCourse = new Course
        {
            Id = 1
        };
        SetUpCourses(new List<Course> { expectedCourse });
        var result = await _courseDao.GetCourseByIdOrThrow(1);

        Assert.Equal(expectedCourse, result);
    }

    [Fact]
    public async Task GetCourseById_ShouldThrowNotFound()
    {
        SetUpCourses(new List<Course>());

        await Assert.ThrowsAsync<NotFoundException>(() => _courseDao.GetCourseByIdOrThrow(2));
    }
}