using System.Threading.Tasks;
using DurableAzureFuncBoilerplate.Upstream.Models;

namespace DurableAzureFuncBoilerplate.Upstream.Clients;

public interface IBoredClient
{
    public Task<BoredActivity> GetActivity(int numberOfParticipants);
}