namespace rag_2_backend.DTO
{
    public class GameResponse
    {
        public int Id { get; set; }
        public required int GameId { get; set; }
        public required string Value { get; set; }
    }
}