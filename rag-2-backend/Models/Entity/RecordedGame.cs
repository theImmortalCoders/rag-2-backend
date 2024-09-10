using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using rag_2_backend.DTO;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.models.entity;

[Table("recorded_games")]
public class RecordedGame
{
    [Key] 
    public int Id { get; init; }
    public required Game Game { get; init; }
    public required User User { get; init; }
    
    private string? _values;
    [NotMapped]
    public required List<RecordedGameResponseValue> Values
    {
        get => JsonConvert.DeserializeObject<List<RecordedGameResponseValue>>(ValuesStr ?? "[]")!;
        set => ValuesStr = JsonConvert.SerializeObject(value);
    }
    [Column("ValuesStr")]
    [MaxLength(10000)]
    public string? ValuesStr { get; set; }
}