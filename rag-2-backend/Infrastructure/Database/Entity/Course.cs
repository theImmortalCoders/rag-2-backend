#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#endregion

namespace rag_2_backend.Infrastructure.Database.Entity;

[Table("course_table")]
[Index(nameof(Name), IsUnique = true)]
public class Course
{
    [Key] public int Id { get; init; }
    [MaxLength(100)] public string Name { get; init; } = "";
}