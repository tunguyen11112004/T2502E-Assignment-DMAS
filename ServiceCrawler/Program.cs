// See https://aka.ms/new-console-template for more information

using ServiceCrawler;

Console.WriteLine("Hello, World!");

var crawler = new CrawlerProducer();

crawler.StartCrawling();