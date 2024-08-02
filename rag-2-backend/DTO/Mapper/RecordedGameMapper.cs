namespace rag_2_backend.DTO.Mapper;

using rag_2_backend.DTO;
using rag_2_backend.models.entity;

public class RecordedGameMapper
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
