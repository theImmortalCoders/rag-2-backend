#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class CourseDao(DatabaseContext dbContext)
{
    public virtual async Task<Course> GetCourseByIdOrThrow(int id)
    {
        return await dbContext.Courses.SingleOrDefaultAsync(u => u.Id == id) ??
               throw new NotFoundException("Course not found");
    }
}