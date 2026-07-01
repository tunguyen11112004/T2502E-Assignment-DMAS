using HtmlAgilityPack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Fizzler.Systems.HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceCrawler.Models;
using ServiceProcessor.Data;

namespace ServiceProcessor;

public class ArticleProcessorConsumer
{
    private const string QueueName = "crawler";
    private readonly AppDbContext _dbContext;
    
    private readonly ILogger<ArticleProcessorConsumer> _logger;
    

    public ArticleProcessorConsumer(AppDbContext dbContext, ILogger<ArticleProcessorConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public void StartProcessing()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(queue: QueueName, durable: true, false, false, null);

        // Giới hạn chỉ nhận 1 tin nhắn mỗi lần (Fair Dispatch)
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) => // 1. Thêm từ khóa async ở đây
        {
            var body = ea.Body.ToArray();
            var link = Encoding.UTF8.GetString(body); // message chính là link

            Console.WriteLine($" [v] Nhận được link: {link} ");

            try
            {
                // 2. GỌI HÀM NÀY ĐỂ XỬ LÝ VÀ LƯU VÀO DB
                await ProcessArticle(link);

                // Xác nhận đã xử lý xong
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] Lỗi khi xử lý {link}: {ex.Message}");
                // Nếu lỗi, đưa về hàng đợi để thử lại
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        Console.WriteLine(" [*] Đang chờ work queue");
        Console.ReadLine();
    }

    public async Task ProcessArticle(string link)
    {
        _logger.LogInformation("Processing url: " + link);
        var webClient = new HtmlWeb();
        var doc = webClient.Load(link);
        var articleNode = doc.DocumentNode.QuerySelector("article");
        var title = doc.DocumentNode.QuerySelector("h1")?.InnerText.Trim();
        var desc = doc.DocumentNode.QuerySelector("p.description")?.InnerText.Trim();
        
        // Remove h1
        articleNode.QuerySelector("h1.title-detail")?.Remove();
        var content = articleNode.InnerHtml;
        var article = new Article
        {
            Title = title ?? "No Title",
            Description = desc ?? "",
            Content = content ?? "",
            Url = link,
            CreatedAt = DateTime.Now
        };
        
        bool exists = await _dbContext.Articles.AnyAsync(a => a.Url == article.Url);
            
        if (!exists)
        {
            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Đã lưu vào Database thành công url: " + link);
        }
        else
        {
            Console.WriteLine("--> Bài viết đã tồn tại, bỏ qua.");
        }

        Console.WriteLine($" [v] Đã bóc tách xong bài: {article.Title}");
    }
}