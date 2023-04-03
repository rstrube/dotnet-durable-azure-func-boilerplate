namespace DurableAzureFuncBoilerplate.Upstream.Models;

/// <summary>
/// The data model used by the upstream API (https://www.boredapi.com/)
/// </summary>
public class BoredActivity
{
    public string Activity { get; set; }
    public string Type { get; set; }
    public int Participants { get; set; } = 0;
    public decimal Price { get; set; } = 0.00M;
    public string Link { get; set; }
    public string Key { get; set; }
    public decimal Accessibility { get; set; } = 0.00M;
}