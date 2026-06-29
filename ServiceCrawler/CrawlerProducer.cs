using System.Net;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using RabbitMQ.Client;
using System.Text;

namespace ServiceCrawler;

public class CrawlerProducer
{
    private const string QueueName= "crawler";
    private const string SourceUrl = "https://vnexpress.net/the-thao";

    public async Task StartCrawling()
    {
        // goi len sourceUrl lay ra cac link bai viet
        var webClient = new HtmlWeb();
        var response = webClient.Load(SourceUrl);
        var linkNodes = response.DocumentNode.QuerySelectorAll("article h3.title-news a");
        // danh sach link
        List<string> links = new List<string>();
        // selector: article h3.title-news a[href]
        if (linkNodes != null)
        {
            foreach (var linkNode in linkNodes)
            {
                links.Add(linkNode.GetAttributeValue("href", ""));
            }
        }
        // goi ham SendQueue
        Console.WriteLine($"Đã tìm thấy {links.Count} link bài viết.");
        SendQueue(links);
    }

    private void SendQueue(List<string> links)
    {
        // dua link baif viet len queue
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        foreach (var link in links)
        {
            // Chuyển đổi link thành mảng byte
            var body = Encoding.UTF8.GetBytes(link);

            channel.BasicPublish(exchange: string.Empty,
                routingKey: QueueName,
                basicProperties: properties,
                body: body);

            Console.WriteLine($" [x] Đã gửi: {link}");
        }
    }
}