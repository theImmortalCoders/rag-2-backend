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

    public async Task AddCourse(CourseRequest request)
    {
        if (await context.Courses.AnyAsync(c => c.Name == request.Name))
            throw new BadRequestException("Course with this name already exists");

        var course = new Database.Entity.Course
        {
            Name = request.Name
        };

        await context.Courses.AddAsync(course);
        await context.SaveChangesAsync();
    }

    public async Task EditCourse(CourseRequest request, int id)
    {
        var course = await courseDao.GetCourseByIdOrThrow(id);

        if (await context.Courses.AnyAsync(c => c.Name == request.Name && c.Name != course.Name))
            throw new BadRequestException("Course with this name already exists");

        course.Name = request.Name;

        context.Courses.Update(course);
        await context.SaveChangesAsync();
    }

    public async Task RemoveCourse(int id)
    {
        var course = await courseDao.GetCourseByIdOrThrow(id);

        var usersWithCourses = await context.Users
            .Include(u => u.Course).CountAsync(u => u.Course != null && u.Course.Id == id);

        if (usersWithCourses > 0)
            throw new BadRequestException("Cannot delete used course");

        context.Courses.Remove(course);
        await context.SaveChangesAsync();
    }
}