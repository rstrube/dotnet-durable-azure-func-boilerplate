namespace DurableAzureFuncBoilerplate.Configuration;

public class BoredClientConfig
{
    public const string Section = "BoredClient";
    public bool UseMock { get; set; } = false;
    public string BaseAddress { get; set; } = string.Empty;
    public string ActivityPath { get; set; } = string.Empty;
}