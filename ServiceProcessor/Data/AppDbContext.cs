using Microsoft.EntityFrameworkCore;
using ServiceCrawler.Models;

namespace ServiceProcessor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Article> Articles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Đảm bảo URL là duy nhất để tránh crawl trùng dữ liệu
        modelBuilder.Entity<Article>()
            .HasIndex(a => a.Url)
            .IsUnique();
    }
    
    
}