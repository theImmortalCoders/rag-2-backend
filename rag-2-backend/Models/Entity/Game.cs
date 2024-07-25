using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Models;

namespace rag_2_backend.models.entity;

[Table("games")]
[Index(nameof(Name), IsUnique = true)]
public class Game
{
    [Key] public int Id { get; init; }
    [MaxLength(100)] public required string Name { get; init; }
    public GameType GameType { get; init; }
}