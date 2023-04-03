using DurableAzureFuncBoilerplate.Upstream.Models;

namespace DurableAzureFuncBoilerplate.Models;

/// <summary>
/// This is the intermediate API model that will be presented to and used by our application.
/// This could be:
/// 1. Front-end Javascript calling the intermediate API directly via an XHR.
/// 2. Back-end code on our web application calling the intermediate API.
/// This particular intermediate API model extrapolates and refines the data returned by the upstream API model (https://www.boredapi.com/).
/// Sometimes you will map the upstream API model to the intermediate API model 1:1 (in terms of properties).
/// In this case it's still important to build a dedicated intermediate API model (in case upstream API models change).
/// </summary>
public class Activity 
{
    public Activity(BoredActivity activity)
    {
        Id = activity.Key;
        ActivityDescription = activity.Activity;
        ActivityType = activity.Type;
        NumberOfParticipants = activity.Participants;
        Price = activity.Price;
        Uri = activity.Link;
        AccessibilityScore = activity.Accessibility;
    }
    
    public string Id { get; set; }
    public string ActivityDescription { get; set; }
    public string ActivityType { get; set; }
    public int NumberOfParticipants { get; set; } = 0;
    public decimal Price { get; set; } = 0.00M;
    public string Uri { get; set; }
    public decimal AccessibilityScore { get; set; } = 0.00M;
    public string AccessibilityRating => AccessibilityScore switch
    {
        > 0.66M => "Hard",
        > 0.33M => "Intermediate",
        _ => "Easy"
    };
}