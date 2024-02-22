namespace kpu_scraper_v2;

public class Location
{
    public string? ProvinceCode { get; set; }
    public string? CityCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? SubdistrictCode { get; set; }
    public string? TpsCode { get; set; }

    public Location(string provinceCode, string? cityCode = null, string? districtCode = null, string? subdistrictCode = null, string? tpsCode = null)
    {
        ProvinceCode = provinceCode;
        CityCode = cityCode;
        DistrictCode = districtCode;
        SubdistrictCode = subdistrictCode;
        TpsCode = tpsCode;
    }

    public string GetLocationCode()
    {
        var result = "";
        result += !string.IsNullOrEmpty(ProvinceCode) ? $"/{ProvinceCode}" : "";
        result += !string.IsNullOrEmpty(CityCode) ? $"/{CityCode}" : "";
        result += !string.IsNullOrEmpty(DistrictCode) ? $"/{DistrictCode}" : "";
        result += !string.IsNullOrEmpty(SubdistrictCode) ? $"/{SubdistrictCode}" : "";
        result += !string.IsNullOrEmpty(TpsCode) ? $"/{TpsCode}" : "";
        return result;
    }

    public Location Clone()
    {
        return new Location(ProvinceCode!, CityCode, DistrictCode, SubdistrictCode, TpsCode);
    }
}