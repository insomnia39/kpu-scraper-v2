namespace kpu_scraper_v2;

public class LocationResult
{
    public string Nama { get; set; }
    public string Id { get; set; }
    public string Kode { get; set; }
    public string Tingkat { get; set; }
    public List<LocationResult> Children { get; set; }

    public List<Location> GetLocations(LocationResult locationResult, Location? location = null)
    {
        var result = new List<Location>();
        switch (locationResult.Tingkat)
        {
            case "1": location = new Location(locationResult.Kode); break;
            case "2": location!.CityCode = locationResult.Kode; break;
            case "3": location!.DistrictCode = locationResult.Kode; break;
            case "4": location!.SubdistrictCode = locationResult.Kode; break;
            case "5": location!.TpsCode = locationResult.Kode; break;
        }
        
        if(locationResult.Children != null)
        {
            foreach (var child in locationResult.Children)
            {
                result.AddRange(GetLocations(child, location));
            }
        }
        else
        {
            result.Add(location.Clone());
        }

        return result;
    }
}