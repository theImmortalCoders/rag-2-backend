#region

using rag_2_backend.Infrastructure.Common.Model;

#endregion

namespace rag_2_backend.Infrastructure.Module.GameRecord.Dto;

public class RecordedGameRequest
{
    public required string GameName { get; init; }
    public required List<GameRecordValue> Values { get; init; }
}