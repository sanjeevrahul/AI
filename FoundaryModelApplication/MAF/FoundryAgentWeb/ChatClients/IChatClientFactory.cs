using Microsoft.Extensions.AI;
using FoundryAgent.Models;

namespace FoundryAgent.ChatClients;

public interface IChatClientFactory
{
    IChatClient Create(ModelConfiguration configuration);
}