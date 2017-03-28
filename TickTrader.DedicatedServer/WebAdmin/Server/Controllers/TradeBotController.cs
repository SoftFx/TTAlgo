using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS;
using System.Linq;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    public class TradeBotController: Controller
    {
        private readonly ILogger<TradeBotController> _logger;
        private readonly IDedicatedServer _dedicatedServer;

        public TradeBotController(IDedicatedServer ddServer, ILogger<TradeBotController> logger)
        {
            _dedicatedServer = ddServer;
            _logger = logger;
        }

        [HttpPost]
        public void Post([FromBody] BotCommand command)
        {
            switch (command.Command.ToLower())
            {
                case "start":
                    StartBot(command.BotId);
                    break;
                case "stop":
                    StopBot(command.BotId);
                    break;
            }
        }

        private void StartBot(string botId)
        {
            var bot = _dedicatedServer.TradeBots.FirstOrDefault(b => b.Id == botId);
            if(bot != null)
            {
                bot.Start();
            }
        }
        private void StopBot(string botId)
        {
            var bot = _dedicatedServer.TradeBots.FirstOrDefault(b => b.Id == botId);
            if (bot != null)
            {
                bot.StopAsync();
            }
        }
    }

    public class BotCommand
    {
        public string Command { get; set; }
        public string BotId { get; set; }
    }
}
