using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rag_2_backend.models;

[Table("games")]
public class Game
{
    [Key] public int Id { get; init; }
    [MaxLength(100)] public required string Name { get; init; }
}