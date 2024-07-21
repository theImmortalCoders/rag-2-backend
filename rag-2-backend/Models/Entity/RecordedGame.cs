using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rag_2_backend.models.entity;

[Table("recorded_games")]
public class RecordedGame
{
    [Key] public int Id { get; init; }
    public required Game Game { get; set; }
    public required string Value { get; set; }
}