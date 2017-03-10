using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.Algo.Common.Model;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    public class TestAccountController
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
            var code = _dedicatedServer.TestAccount(account.Login, account.Password, account.Server);
            return code;
        }
    }
}
