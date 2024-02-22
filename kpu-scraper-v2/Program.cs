namespace kpu_scraper_v2;

public class Program
{
    public static void Main()
    {
        var kpuScraperV2 = new KpuScraperV2();
        kpuScraperV2.RunAsync().Wait();
    }
}