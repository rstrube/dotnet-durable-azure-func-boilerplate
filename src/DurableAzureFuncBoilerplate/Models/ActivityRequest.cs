using System.Collections.Generic;

namespace DurableAzureFuncBoilerplate.Models;

public class ActivityRequest
{
    public List<int> NumberOfParticipants { get; set; }
}