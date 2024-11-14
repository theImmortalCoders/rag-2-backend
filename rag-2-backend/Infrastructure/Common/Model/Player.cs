#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace rag_2_backend.Infrastructure.Common.Model;

// ReSharper disable once ClassNeverInstantiated.Global
public class Player
{
    public required int Id { get; set; }
    [MaxLength(100)] public required string Name { get; set; }
    public required bool IsObligatory { get; set; }
    public required bool IsActive { get; set; }
    public required PlayerType PlayerType { get; set; }
    public object? InputData { get; set; }
    [MaxLength(500)] public string? ExpectedDataDescription { get; set; }
}