using Microsoft.EntityFrameworkCore;
using ServiceCrawler.Models;

namespace ServiceProcessor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Article> Articles { get; set; }
    
    
    
}