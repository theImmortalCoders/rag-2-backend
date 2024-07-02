namespace rag_2_backend.models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class User
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")] public string Name { get; set; }
}