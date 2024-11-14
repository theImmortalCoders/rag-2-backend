#region

using System.ComponentModel.DataAnnotations;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Module.Game.Dto;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.GameRecord.Dto;

public class GameRecordJsonResponse
{
    public int Id { get; init; }
    public required GameResponse Game { get; init; }
    public required UserResponse User { get; init; }
    public required List<GameRecordValue> Values { get; init; }
    public List<Player>? Players { get; init; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    [MaxLength(1000)] public string? OutputSpec { get; init; }
    [MaxLength(500)] public string? EndState { get; init; }
    public double SizeMb { get; init; }
}