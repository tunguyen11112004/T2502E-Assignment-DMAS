using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCrawler.Models;

public class Article
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "text")] // Sử dụng kiểu text cho mô tả
    public string? Description { get; set; }

    [Column(TypeName = "longtext")] // Sử dụng longtext cho nội dung bài viết dày
    public string? Content { get; set; }

    [Required]
    public string Url { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}