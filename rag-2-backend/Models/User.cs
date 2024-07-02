using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rag_2_backend.models;

[Table("users")]
public class User
{
    [Column("id")] public int Id { get; init; }

    [MaxLength(100)] [Column("name")] public required string Name { get; init; }
}