using Newtonsoft.Json;

namespace kpu_scraper_v2;

public class KpuScraperV2
{
    private readonly string _currentDate = DateTime.Now.ToString("yyyy-MM-dd");
    private const string BaseLocationUrl = "https://sirekap-obj-data.kpu.go.id/wilayah/pemilu/ppwp";
    private const string BaseVoteUrl = "https://sirekap-obj-data.kpu.go.id/pemilu/hhcw/ppwp";
    private static readonly HttpClient HttpClient = new HttpClient();
    private readonly List<LocationResult> _filteredLocations = new List<LocationResult>();

    public KpuScraperV2(List<LocationResult>? filteredLocations = null)
    {
        if (filteredLocations is { Count: > 0 }) _filteredLocations = filteredLocations;
    }

    public async Task RunAsync()
    {
        var locations = await GetInitLocations();
        var filteredLocationNames = _filteredLocations.Select(p => p.Nama);
        var filteredLocations = _filteredLocations.Any() ? locations.Where(location => filteredLocationNames.Contains(location.Nama)).ToList() : locations;
        var allLocations = await GetAllLocations(filteredLocations);
        await GetAllVotes(allLocations);
    }

    private async Task<List<LocationResult>> GetInitLocations()
    {
        var locations = FileManager.OpenJson<List<LocationResult>>("provinces.json");
        if (locations is { Count: > 0 }) return locations;
        locations = await GetLocationAsync(new Location("0"));
        FileManager.SaveJson("provinces.json", locations);
        return locations;
    }

    private async Task<List<LocationResult>> GetLocationAsync(Location location, IProgress<int>? progress = null)
    {
        var url = $"{BaseLocationUrl}{location.GetLocationCode()}.json";
        var response = await HttpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) throw new Exception(response.ToString());
        var content = await response.Content.ReadAsStringAsync();
        progress?.Report(1);
        return JsonConvert.DeserializeObject<List<LocationResult>>(content) ?? new List<LocationResult>();
    }

    private async Task<dynamic?> GetVoteAsync(Location location, IProgress<int>? progress = null)
    {
        var url = $"{BaseVoteUrl}{location.GetLocationCode()}.json";
        var response = await HttpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) throw new Exception(response.ToString());
        var content = await response.Content.ReadAsStringAsync();
        progress?.Report(1);
        return JsonConvert.DeserializeObject<dynamic>(content);
    }

    private async Task<List<LocationResult>> GetAllLocations(List<LocationResult> locations)
    {
        const string functionName = "GetLocation";
        foreach (var province in locations)
        {
            try
            {
                var existingLocation = FileManager.OpenJson<LocationResult>(Path.Combine("location", $"{province.Nama}.json"));
                if (existingLocation != null)
                {
                    province.Children = existingLocation.Children;
                    continue;
                }
            } catch (Exception e) { Console.WriteLine(e.Message); }
            var cities = await GetLocationAsync(new Location(province.Kode));
            province.Children = cities;
            var districtTasks = new List<Task<List<LocationResult>>>();
            districtTasks.AddRange(cities.Select(city => GetLocationAsync(new Location(province.Kode, city.Kode))));
            await Task.WhenAll(districtTasks);
            var districts = districtTasks.SelectMany(p => p.Result).ToList();
            var districtLocations = new List<Location>();
            foreach (var city in cities)
            {
                var filteredData = districts.Where(p => p.Kode.StartsWith(city.Kode)).ToList();
                city.Children = filteredData;
                districtLocations.AddRange(filteredData.Select(p => new Location(province.Kode, city.Kode, p.Kode)));
            }
            
            var subdistrictTasks = new List<Task<List<LocationResult>>>();
            var subdistrictProgress = new BatchProgress($"{functionName} : {province.Nama} Subdistrict", districtLocations.Count);
            subdistrictTasks.AddRange(districtLocations.Select(district => GetLocationAsync(district, subdistrictProgress)));
            await Task.WhenAll(subdistrictTasks);
            Console.WriteLine();
            var subdistricts = subdistrictTasks.SelectMany(p => p.Result).ToList();
            var subdistrictLocations = new List<Location>();
            foreach (var city in cities)
            {
                foreach (var district in city.Children)
                {
                    var filteredData = subdistricts.Where(p => p.Kode.StartsWith(district.Kode)).ToList();
                    district.Children = filteredData;
                    subdistrictLocations.AddRange(filteredData.Select(p => new Location(province.Kode, city.Kode, district.Kode, p.Kode)));
                }
            }
            
            var tpsTasks = new List<Task<List<LocationResult>>>();
            var tpsProgress = new BatchProgress($"{functionName} : {province.Nama} TPS", subdistrictLocations.Count); // Track progress
            tpsTasks.AddRange(subdistrictLocations.Select(subdistrict => GetLocationAsync(subdistrict, tpsProgress)));
            await Task.WhenAll(tpsTasks);
            Console.WriteLine();
            var tpss = tpsTasks.SelectMany(p => p.Result).ToList();
            var processPorgress = new BatchProgress($"{functionName} : {province.Nama} Processing", subdistrictLocations.Count);
            foreach (var city in cities)
            {
                foreach (var district in city.Children)
                {
                    foreach (var subdistrict in district.Children)
                    {
                        var filteredData = tpss.Where(p => p.Kode.StartsWith(subdistrict.Kode)).ToList();
                        subdistrict.Children = filteredData;
                        processPorgress.Report(1);
                    }
                }
            }
            FileManager.SaveJson(Path.Combine("location", $"{province.Nama}.json"), province);
        }
        return locations;
    }

    private async Task GetAllVotes(List<LocationResult> locationResults)
    {
        const string functionName = "GetAllVotes";
        foreach (var locationResult in locationResults)
        {
            var locations = locationResult.GetLocations(locationResult);
            var voteTasks = new List<Task<dynamic>>();
            var voteProgress = new BatchProgress($"{functionName} : {locationResult.Nama} ", locations.Count);
            voteTasks.AddRange(locations.ToList().Select(p => GetVoteAsync(p, voteProgress))!);
            await Task.WhenAll(voteTasks);
            Console.WriteLine();
            var votes = voteTasks.Select(p => p.Result).ToList();
            FileManager.SaveJson(Path.Combine("result", _currentDate, $"{locationResult.Nama}.json"), votes);
        }
    }
}