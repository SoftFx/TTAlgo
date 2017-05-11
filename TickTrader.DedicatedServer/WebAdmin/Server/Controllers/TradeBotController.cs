using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;
using TickTrader.DedicatedServer.WebAdmin.Server.Models;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TradeBotController : Controller
    {
        private readonly ILogger<TradeBotController> _logger;
        private readonly IDedicatedServer _dedicatedServer;

        public TradeBotController(IDedicatedServer ddServer, ILogger<TradeBotController> logger)
        {
            _dedicatedServer = ddServer;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] BotCommand command)
        {
            try
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
            catch(DSException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        private void StartBot(string botId)
        {
            var bot = _dedicatedServer.TradeBots.FirstOrDefault(b => b.Id == botId);
            if (bot != null)
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
}
