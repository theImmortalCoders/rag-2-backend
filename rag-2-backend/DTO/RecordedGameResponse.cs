namespace rag_2_backend.DTO
{
    public class RecordedGameResponse{
        public int Id { get; init; }
        public required int GameId { get; init; }
        public required string Value { get; init; }
    }
}