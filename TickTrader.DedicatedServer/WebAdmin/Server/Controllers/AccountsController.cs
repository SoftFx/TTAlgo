using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using TickTrader.Algo.Common.Model;
using TickTrader.DedicatedServer.DS.Info;
using System.Net;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ILogger<PackagesController> _logger;
        private readonly IDedicatedServer _dedicatedServer;

        public AccountsController(IDedicatedServer ddServer, ILogger<PackagesController> logger)
        {
            _dedicatedServer = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public AccountDto[] Get()
        {
            return _dedicatedServer.Accounts.Select(a => a.ToDto()).ToArray();
        }

        [HttpGet("{server}/{login}/[action]")]
        public IActionResult Info(string server, string login)
        {
            try
            {
                var connErrorCode = _dedicatedServer.GetAccountInfo(new AccountKey(WebUtility.UrlDecode(login), WebUtility.UrlDecode(server)), out ConnectionInfo info);

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
            catch (DSException dsex)
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
                _dedicatedServer.AddAccount(new AccountKey(account.Login, account.Server), account.Password);
            }
            catch (DSException dsex)
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
                _dedicatedServer.RemoveAccount(new AccountKey(login ?? "", server ?? ""));
            }
            catch (DSException dsex)
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
                _dedicatedServer.ChangeAccountPassword(new AccountKey(account.Login, account.Server), account.Password);
            }
            catch (DSException dsex)
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
                    _dedicatedServer.TestAccount(new AccountKey(login, server)) :
                    _dedicatedServer.TestCreds(login, password, server);

                return Ok(testResult);
            }
            catch (DSException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }
        }
    }
}
