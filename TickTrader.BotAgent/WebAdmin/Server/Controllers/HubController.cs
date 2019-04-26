using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TickTrader.BotAgent.WebAdmin.Server.Hubs;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    public abstract class HubController<T>: Controller where T: Hub
    {
        private readonly IHubContext<BAFeed, IBAFeed> _hub;


        public IHubClients<IBAFeed> Clients { get; private set; }
        public IGroupManager Groups { get; private set; }


        protected HubController(IHubContext<BAFeed, IBAFeed> hub)
        {
            _hub = hub;

            Clients = _hub.Clients;
            Groups = _hub.Groups;
        }
    }
}
