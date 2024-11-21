#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        var coursesQueryable = courses.AsQueryable();
        var coursesDbSetMock = new Mock<DbSet<Course>>();
        coursesDbSetMock.As<IQueryable<Course>>().Setup(m => m.Provider).Returns(coursesQueryable.Provider);
        coursesDbSetMock.As<IQueryable<Course>>().Setup(m => m.Expression).Returns(coursesQueryable.Expression);
        coursesDbSetMock.As<IQueryable<Course>>().Setup(m => m.ElementType).Returns(coursesQueryable.ElementType);
        using var enumerator = coursesQueryable.GetEnumerator();
        coursesDbSetMock.As<IQueryable<Course>>().Setup(m => m.GetEnumerator()).Returns(enumerator);

        _dbContextMock.Setup(db => db.Courses).Returns(coursesDbSetMock.Object);
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