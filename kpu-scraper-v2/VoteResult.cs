namespace kpu_scraper_v2;

public class VoteResult
{
    public string? Mode { get; set; }
    public Chart? Chart { get; set; }
    public List<string>? Images { get; set; }
    public Administrasi? Administrasi { get; set; }
    public object? Psu { get; set; }
    public string? Ts { get; set; }
    public bool? StatusSuara { get; set; }
    public bool? StatusAdm { get; set; }
}
public class Chart
{
    public object? Null { get; set; }
    public int? _100025 { get; set; }
    public int? _100026 { get; set; }
    public int? _100027 { get; set; }
}

public class Administrasi
{
    public int? SuaraSah { get; set; }
    public int? SuaraTotal { get; set; }
    public int? PemilihDptJ { get; set; }
    public int? PemilihDptL { get; set; }
    public int? PemilihDptP { get; set; }
    public int? PenggunaDptJ { get; set; }
    public int? PenggunaDptL { get; set; }
    public int? PenggunaDptP { get; set; }
    public int? PenggunaDptbJ { get; set; }
    public int? PenggunaDptbL { get; set; }
    public int? PenggunaDptbP { get; set; }
    public int? SuaraTidakSah { get; set; }
    public int? PenggunaTotalJ { get; set; }
    public int? PenggunaTotalL { get; set; }
    public int? PenggunaTotalP { get; set; }
    public int? PenggunaNonDptJ { get; set; }
    public int? PenggunaNonDptL { get; set; }
    public int? PenggunaNonDptP { get; set; }
}