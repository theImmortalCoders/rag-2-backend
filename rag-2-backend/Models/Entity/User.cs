using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using rag_2_backend.Services;

namespace rag_2_backend.Models.Entity;

[Table("users")]
public class User
{
    [Key] public int Id { get; init; }
    [MaxLength(100)] public string Email { get; init; } = "";
    [MaxLength(100)] public required string Password { get; init; }
    public Role Role { get; set; }
    public bool Confirmed { get; set; }
    public int StudyCycleYearA { get; init; }
    public int StudyCycleYearB { get; init; }

    public User() //for ef
    {
    }

    public User(string email)
    {
        var domain = email.Split('@')[1];

        if (!domain.Equals("stud.prz.edu.pl") && !domain.Equals("prz.edu.pl"))
            throw new BadHttpRequestException("Wrong domain");

        Role = domain.Equals("stud.prz.edu.pl") ? Role.Student : Role.Teacher;
        Email = email;
    }
}