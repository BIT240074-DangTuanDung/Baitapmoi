using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaithuchanhORM.Models;

public sealed class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Publisher { get; set; }

    [Range(0, 10_000_000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(0, 10_000)]
    public int Stock { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PublishedDate { get; set; }

    public ICollection<BookImage> Images { get; set; } = [];
}

