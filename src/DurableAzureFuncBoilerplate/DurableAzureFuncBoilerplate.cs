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
using System.Threading;

namespace DurableAzureFuncBoilerplate;

public class DurableAzureFuncBoilerplate
{
    [FunctionName(nameof(RunOrchestrator))]
    public async Task<List<object>> RunOrchestrator(
        [OrchestrationTrigger]
            IDurableOrchestrationContext context,
        ILogger log)
    {
        var activityRequest = context.GetInput<ActivityRequest>();

        log.LogInformation($"Starting RunOrchestrator for:\n{JsonSerializer.Serialize(activityRequest, new JsonSerializerOptions() { WriteIndented = true })}");

        var outputs = new List<object>();

        foreach(var participants in activityRequest.NumberOfParticipants)
        {
            var activity = await context.CallActivityAsync<object>(nameof(GetActivitySuggestion), participants);
            outputs.Add(activity);
        }

        return outputs;
    }

    [FunctionName(nameof(GetActivitySuggestion))]
    public async Task<object> GetActivitySuggestion(
        [ActivityTrigger]
            int participants,
        ILogger log)
    {
        log.LogInformation($"Starting GetActivitySuggestion for participants: {participants}");

        var boredActivity = await _boredClient.GetActivity(participants);

        if(boredActivity == null)
        {
            log.LogError("Received null suggestion from upstream API");
            return null;
        }

        log.LogInformation($"Received suggestion, upstream model:\n{JsonSerializer.Serialize(boredActivity, new JsonSerializerOptions() { WriteIndented = true })}");

        var activity = new Activity(boredActivity);

        log.LogInformation($"After transformation, web application model:\n{JsonSerializer.Serialize(activity, new JsonSerializerOptions() { WriteIndented = true })}");

        //add in a slight delay 
        await Task.Delay(5000);

        return activity;
    }

    [FunctionName(nameof(DurableAzureFuncBoilerplate_HttpStart))]
    public async Task<HttpResponseMessage> DurableAzureFuncBoilerplate_HttpStart(
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

    public DurableAzureFuncBoilerplate(IOptions<BoredClientConfig> boredClientConfig, IBoredClient boredClient)
    {
        _boredClientConfig = boredClientConfig.Value;
        _boredClient = boredClient;
    }
}