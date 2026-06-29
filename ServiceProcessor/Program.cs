using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServiceProcessor.Data;
using System.IO;
using ServiceProcessor;

Console.WriteLine("Hệ thống Consumer đã khởi động...");

// Đọc cấu hình từ appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

using (var dbContext = new AppDbContext(optionsBuilder.Options))
{
    // Lệnh này sẽ tự động tạo Database Dbqueue và bảng Articles dựa trên appsettings.json
    dbContext.Database.EnsureCreated(); 

    var consumer = new ArticleProcessorConsumer(dbContext);
    consumer.StartProcessing();
}