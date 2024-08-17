namespace rag_2_backend.DTO;

public class RecordedGameResponse
{
    public int Id { get; set; }
    public required UserResponse UserResponse { get; set; }
    public required GameResponse GameResponse { get; set; }
    public required string Value { get; set; }
}