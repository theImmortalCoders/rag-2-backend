#region

using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Course.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Common.Mapper;

public static class CourseMapper
{
    public static CourseResponse Map(Course course)
    {
        return new CourseResponse
        {
            Id = course.Id,
            Name = course.Name
        };
    }
}