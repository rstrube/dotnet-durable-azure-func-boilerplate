using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DurableAzureFuncBoilerplate.Upstream.Models;
using DurableAzureFuncBoilerplate.Configuration;
using Microsoft.Extensions.Options;

namespace DurableAzureFuncBoilerplate.Upstream.Clients;

/// <summary>
/// Client for the upstream API (https://www.boredapi.com/)
/// </summary>
public class BoredClient : IBoredClient
{
    // see: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines
    // it's highly recommented to use PooledConnectionLifetime to rebuild HttpClient at set intervals
    // to account for DNS changes
    private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    });

    public async Task<BoredActivity> GetActivity(int numOfParticipants)
    {
        var activityPath = _boredClientConfig.Value.ActivityPath;

        if (string.IsNullOrEmpty(activityPath))
        {
            throw new Exception("Invalid BoredClient:ActivityPath configuration");
        }

        if(numOfParticipants > 0)
        {
            activityPath = $"{activityPath}?participants={numOfParticipants}";
        }

        var response = await _httpClient.GetAsync(activityPath);

        if(!response.IsSuccessStatusCode)
        {
            return default(BoredActivity);
        }

        var rawString = await response.Content.ReadAsStringAsync();
        return await response.Content.ReadFromJsonAsync<BoredActivity>();
    }

    private readonly IOptions<BoredClientConfig> _boredClientConfig;

    public BoredClient(IOptions<BoredClientConfig> boredClientConfig)
    {
        _boredClientConfig = boredClientConfig;

        if (!Uri.TryCreate(_boredClientConfig.Value.BaseAddress, UriKind.Absolute, out var uri))
        {
            throw new Exception("Invalid BoredClient:BaseAddress configuration");
        }

        _httpClient.BaseAddress = uri;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}