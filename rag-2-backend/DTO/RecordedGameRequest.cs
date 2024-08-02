namespace rag_2_backend.DTO;

public class RecordedGameRequest
{
    public required string Value { get; set; }
    public required UserResponse UserResponse { get; set; }
    public required GameResponse GameResponse { get; set; }
}
