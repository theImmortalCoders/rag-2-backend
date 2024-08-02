using rag_2_backend.Models;

namespace rag_2_backend.DTO;
public class RecordedGameResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public GameType GameType { get; set; }
}
