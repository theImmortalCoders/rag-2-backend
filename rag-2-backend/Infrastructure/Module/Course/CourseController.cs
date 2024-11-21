#region

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rag_2_backend.Infrastructure.Module.Course.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.Course;

[ApiController]
[Route("api/[controller]")]
public class CourseController(CourseService courseService) : ControllerBase
{
    /// <summary>Get courses available in system</summary>
    [HttpGet]
    public async Task<IEnumerable<CourseResponse>> GetCourses()
    {
        return await courseService.GetCourses();
    }

    /// <summary>Add new course to system (Admin)</summary>
    /// <response code="400">Course with this name already exists</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task Add([FromBody] [Required] CourseRequest request)
    {
        await courseService.AddCourse(request);
    }

    /// <summary>Edit existing course (Admin)</summary>
    /// <response code="404">Course not found</response>
    /// <response code="400">Course with this name already exists</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task Edit([FromBody] [Required] CourseRequest request, int id)
    {
        await courseService.EditCourse(request, id);
    }

    /// <summary>Remove existing course (Admin)</summary>
    /// <response code="404">Course not found</response>
    /// <response code="400">Cannot delete used course</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task Remove([Required] int id)
    {
        await courseService.RemoveCourse(id);
    }
}