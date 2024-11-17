#region

using HttpExceptions.Exceptions;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class CourseDao(DatabaseContext dbContext)
{
    public virtual Course GetCourseByIdOrThrow(int id)
    {
        return dbContext.Courses.SingleOrDefault(u => u.Id == id) ??
               throw new NotFoundException("Course not found");
    }

    public virtual List<Course> GetAllCourses()
    {
        return dbContext.Courses.ToList();
    }
}