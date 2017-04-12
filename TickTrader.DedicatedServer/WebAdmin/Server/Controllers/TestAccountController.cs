using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.Algo.Common.Model;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TestAccountController: Controller
    {
        private readonly ILogger<RepositoryController> _logger;
        private readonly IDedicatedServer _dedicatedServer;

        public TestAccountController(IDedicatedServer ddServer, ILogger<RepositoryController> logger)
        {
            _dedicatedServer = ddServer;
            _logger = logger;
        }

        [HttpPost]
        public ConnectionErrorCodes Post([FromBody]AccountDto account)
        {
            return string.IsNullOrWhiteSpace(account.Password) ?
                _dedicatedServer.TestAccount(new AccountKey(account.Login, account.Server)) :
                _dedicatedServer.TestCreds(account.Login, account.Password, account.Server);
        }
    }
}
