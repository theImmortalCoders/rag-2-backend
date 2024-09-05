using rag_2_backend.models.entity;

namespace rag_2_backend.DTO.Mapper;

public abstract class RecordedGameMapper
{
    public static RecordedGameResponse Map(RecordedGame recordedGame)
    {
        return new RecordedGameResponse
        {
            Id = recordedGame.Id,
            UserResponse = UserMapper.Map(recordedGame.User),
            GameResponse = GameMapper.Map(recordedGame.Game),
            Value = recordedGame.Value
        };
    }
}