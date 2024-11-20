#region

using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Module.User.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.GameRecord.Dto;

public class GameRecordResponse
{
    public int Id { get; set; }
    public required string GameName { get; set; }
    public required UserResponse User { get; set; }
    public List<Player>? Players { get; set; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    public string? OutputSpec { get; set; }
    public object? EndState { get; set; }
    public double SizeMb { get; init; }
    public bool IsEmptyRecord { get; init; }
}