using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using TickTrader.Algo.Common.Model;
using TickTrader.BotAgent.BA.Info;
using System.Net;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ILogger<PackagesController> _logger;
        private readonly IBotAgent _botAgent;

        public AccountsController(IBotAgent ddServer, ILogger<PackagesController> logger)
        {
            _botAgent = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public AccountDto[] Get()
        {
            return _botAgent.Accounts.Select(a => a.ToDto()).ToArray();
        }

        [HttpGet("{server}/{login}/[action]")]
        public IActionResult Info(string server, string login)
        {
            try
            {
                var connErrorCode = _botAgent.GetAccountInfo(new AccountKey(WebUtility.UrlDecode(login), WebUtility.UrlDecode(server)), out ConnectionInfo info);

                if (connErrorCode == ConnectionErrorCodes.None)
                {
                    return Ok(info.ToDto());
                }
                else
                {
                    var communicationExc = new CommunicationException($"Connection error: {connErrorCode}", connErrorCode);

                    _logger.LogError(communicationExc.Message);

                    return BadRequest(communicationExc.ToBadResult());
                }
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]AccountDto account)
        {
            try
            {
                _botAgent.AddAccount(new AccountKey(account.Login, account.Server), account.Password);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete(string login, string server)
        {
            try
            {
                _botAgent.RemoveAccount(new AccountKey(login ?? "", server ?? ""));
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpPatch]
        public IActionResult UpdatePassword([FromBody] AccountDto account)
        {
            try
            {
                _botAgent.ChangeAccountPassword(new AccountKey(account.Login, account.Server), account.Password);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult Test(string login, string server, string password)
        {
            try
            {
                var testResult = string.IsNullOrWhiteSpace(password) ?
                    _botAgent.TestAccount(new AccountKey(login, server)) :
                    _botAgent.TestCreds(login, password, server);

                return Ok(testResult);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }
        }
    }
}
