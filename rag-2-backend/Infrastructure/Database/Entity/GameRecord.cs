#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using rag_2_backend.Infrastructure.Common.Model;

#endregion

namespace rag_2_backend.Infrastructure.Database.Entity;

[Table("game_record_table")]
public class GameRecord
{
    [Key] public int Id { get; init; }
    public required Game Game { get; init; }
    public required User User { get; init; }
    public required List<GameRecordValue> Values { get; init; }
    public List<Player>? Players { get; init; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    [MaxLength(1000)] public string? OutputSpec { get; init; }
    [MaxLength(500)] public string? EndState { get; init; }
    public double SizeMb { get; init; }
    public bool IsEmptyRecord { get; init; }
}