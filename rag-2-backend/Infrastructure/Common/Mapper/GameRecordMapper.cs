#region

using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Common.Mapper;

public abstract class GameRecordMapper
{
    public static GameRecordResponse Map(GameRecord gameRecord)
    {
        return new GameRecordResponse
        {
            Id = gameRecord.Id,
            Players = gameRecord.Players,
            Ended = gameRecord.Ended,
            Started = gameRecord.Started,
            EndState = gameRecord.EndState,
            OutputSpec = gameRecord.OutputSpec
        };
    }
}