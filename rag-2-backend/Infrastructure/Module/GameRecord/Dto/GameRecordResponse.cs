#region

using rag_2_backend.Infrastructure.Common.Model;

#endregion

namespace rag_2_backend.Infrastructure.Module.GameRecord.Dto;

public class GameRecordResponse
{
    public int Id { get; set; }
    public List<Player>? Players { get; set; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    public string? OutputSpec { get; set; }
    public object? EndState { get; set; }
}