using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using TickTrader.Algo.Common.Model;

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
        public void Delete(string login, string server)
        {
            _dedicatedServer.RemoveAccount(new AccountKey(login ?? "", server ?? ""));
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
        public ConnectionErrorCodes Test(string login, string server, string password)
        {
            return string.IsNullOrWhiteSpace(password) ?
                _dedicatedServer.TestAccount(new AccountKey(login, server)) :
                _dedicatedServer.TestCreds(login, password, server);
        }
    }
}
