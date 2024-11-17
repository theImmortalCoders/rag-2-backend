#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.Course.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Course;

public class CourseService(DatabaseContext context, CourseDao courseDao)
{
    public async Task<IEnumerable<CourseResponse>> GetCourses()
    {
        var courses = await context.Courses.ToListAsync();

        return courses.Select(CourseMapper.Map);
    }

    public void AddCourse(CourseRequest request)
    {
        if (context.Courses.Any(c => c.Name == request.Name))
            throw new BadRequestException("Course with this name already exists");

        var course = new Database.Entity.Course
        {
            Name = request.Name
        };

        context.Courses.Add(course);
        context.SaveChanges();
    }

    public void EditCourse(CourseRequest request, int id)
    {
        var course = courseDao.GetCourseByIdOrThrow(id);

        if (context.Courses.Any(c => c.Name == request.Name && c.Name != course.Name))
            throw new BadRequestException("Course with this name already exists");

        course.Name = request.Name;

        context.Courses.Update(course);
        context.SaveChanges();
    }

    public void RemoveCourse(int id)
    {
        var course = courseDao.GetCourseByIdOrThrow(id);

        var usersWithCourses = context.Users
            .Include(u => u.Course).Count(u => u.Course != null && u.Course.Id == id);

        if (usersWithCourses > 0)
            throw new BadRequestException("Cannot delete used course");

        context.Courses.Remove(course);
        context.SaveChanges();
    }
}