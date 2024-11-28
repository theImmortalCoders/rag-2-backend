#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Model;

#endregion

namespace rag_2_backend.Infrastructure.Database.Entity;

[Table("user_table")]
[Index(nameof(Email), IsUnique = true)]
public class User
{
    public User() //for ef
    {
    }

    public User(string email)
    {
        string domain;
        try
        {
            domain = email.Split('@')[1];
        }
        catch (Exception)
        {
            throw new BadRequestException("Invalid email address");
        }

        if (!domain.Equals("stud.prz.edu.pl") && !domain.Equals("prz.edu.pl"))
            throw new BadRequestException("Wrong domain");

        Role = domain.Equals("stud.prz.edu.pl") ? Role.Student : Role.Teacher;
        Email = email;
    }

    [Key] public int Id { get; init; }
    [MaxLength(100)] public string Email { get; init; } = "";
    [MaxLength(100)] public required string Password { get; set; }
    [MaxLength(100)] public required string Name { get; set; }
    public Role Role { get; set; }
    public bool Confirmed { get; set; }
    public int StudyCycleYearA { get; set; }
    public int StudyCycleYearB { get; set; }
    public bool Banned { get; set; }
    public DateTime LastPlayed { get; set; }

    [ForeignKey("CourseId")] public Course? Course { get; set; }

    [MaxLength(100)] public string? Group { get; set; }
}