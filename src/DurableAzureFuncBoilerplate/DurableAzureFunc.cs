using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DurableAzureFuncBoilerplate.Configuration;
using DurableAzureFuncBoilerplate.Upstream.Clients;
using System.Text.Json;
using System.Net.Http;
using System.Net;
using System;
using DurableAzureFuncBoilerplate.Models;

namespace DurableAzureFuncBoilerplate;

public class DurableAzureFunc
{
    [FunctionName(nameof(RunOrchestrator))]
    public async Task<ActivityResponse> RunOrchestrator(
        [OrchestrationTrigger]
            IDurableOrchestrationContext context,
        ILogger log)
    {
        var activityRequest = context.GetInput<ActivityRequest>();

        var outputs = new List<Activity>();

        foreach(var participants in activityRequest.NumberOfParticipants)
        {
            outputs.Add(await context.CallActivityAsync<Activity>(nameof(GetActivitySuggestion), participants));
        }

        return new ActivityResponse()
        {
            Activities = outputs
        };
    }

    [FunctionName(nameof(GetActivitySuggestion))]
    public async Task<Activity> GetActivitySuggestion(
        [ActivityTrigger]
            int participants,
        ILogger log)
    {
        log.LogInformation($"Starting GetActivitySuggestion for participants:{participants}");

        var boredActivity = await _boredClient.GetActivity(participants);

        return new Activity(boredActivity);
    }

    [FunctionName(nameof(DurableAzureFunc_HttpStart))]
    public async Task<HttpResponseMessage> DurableAzureFunc_HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestMessage request,
        [DurableClient]
            IDurableOrchestrationClient starter,
        ILogger log)
    {
        try
        {
            var requestStr = await request.Content.ReadAsStringAsync();

            log.LogInformation($"Received POST request:\n{requestStr}");

            var activityRequest = await request.Content.ReadAsAsync<ActivityRequest>();

            if (activityRequest == null)
            {
                log.LogError($"Unable to deserialize ActivityRequest from POST request!");

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            log.LogInformation($"After deserialization to ActivityRequest:\n{JsonSerializer.Serialize(activityRequest, new JsonSerializerOptions() { WriteIndented = true })}");

            string instanceId = await starter.StartNewAsync(nameof(RunOrchestrator), activityRequest);

            log.LogInformation($"Started orchestration with ID: {instanceId}");

            return starter.CreateCheckStatusResponse(request, instanceId);
        }

        catch (Exception e)
        {
            log.LogCritical($"Exception thrown in Azure Function:\n{e}\n{e.StackTrace}");

            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }

    private BoredClientConfig _boredClientConfig;
    private IBoredClient _boredClient;

    public DurableAzureFunc(IOptions<BoredClientConfig> boredClientConfig, IBoredClient boredClient)
    {
        _boredClientConfig = boredClientConfig.Value;
        _boredClient = boredClient;
    }
}