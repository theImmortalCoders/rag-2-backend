using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using rag_2_backend.DTO;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.models.entity;

[Table("recorded_games")]
public class RecordedGame
{
    [Key] public int Id { get; init; }
    public required Game Game { get; init; }
    public required User User { get; init; }
    public required List<RecordedGameValue> Values { get; init; }
    public List<Player>? Players { get; init; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    [MaxLength(1000)] public string? OutputSpec { get; init; }
    [MaxLength(500)] public string? EndState { get; init; }
}