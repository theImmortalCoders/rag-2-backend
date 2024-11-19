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
            GameName = gameRecord.Game.Name,
            Players = gameRecord.Players,
            Ended = gameRecord.Ended,
            Started = gameRecord.Started,
            EndState = gameRecord.EndState,
            OutputSpec = gameRecord.OutputSpec,
            SizeMb = gameRecord.SizeMb,
            User = UserMapper.Map(gameRecord.User),
            IsEmptyRecord = gameRecord.IsEmptyRecord
        };
    }

    public static GameRecordJsonResponse JsonMap(GameRecord gameRecord)
    {
        return new GameRecordJsonResponse
        {
            Id = gameRecord.Id,
            Game = GameMapper.Map(gameRecord.Game),
            User = UserMapper.Map(gameRecord.User),
            Players = gameRecord.Players,
            Ended = gameRecord.Ended,
            Started = gameRecord.Started,
            EndState = gameRecord.EndState,
            OutputSpec = gameRecord.OutputSpec,
            SizeMb = gameRecord.SizeMb,
            Values = gameRecord.Values
        };
    }
}