using Newtonsoft.Json;

namespace kpu_scraper_v2;

public static class FileManager
{
    public static T? OpenJson<T>(string address)
    {
        try
        {
            using var r = new StreamReader(address);
            var json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            throw;
        }
    }
    
    public static void SaveJson<T>(string address, T obj)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(address);
            if(directoryPath == null) throw new Exception("Directory path is null");
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            var json = JsonConvert.SerializeObject(obj);
            using var w = new StreamWriter(address);  
            w.Write(json);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            throw;
        }
    }
}