using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PackagesController : Controller
    {
        private readonly ILogger<PackagesController> _logger;
        private readonly IBotAgent _botAgent;

        public PackagesController(IBotAgent ddServer, ILogger<PackagesController> logger)
        {
            _botAgent = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public PackageDto[] Get()
        {
            var packages = _botAgent.GetPackages();

            return packages.Select(p => p.ToDto()).ToArray();
        }

        [HttpHead("{name}")]
        public IActionResult Head(string name)
        {
            var packages = _botAgent.GetPackages();

            if (_botAgent.GetPackage(name) != null)
                return Ok();

            return NotFound();
        }

        [HttpPost]
        public IActionResult Post(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException("File is null");
            if (file.Length == 0) throw new ArgumentException("File is empty");

            using (var stream = file.OpenReadStream())
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    var fileContent = binaryReader.ReadBytes((int)file.Length);
                    try
                    {
                        _botAgent.UpdatePackage(fileContent, file.FileName);
                    }
                    catch (BAException dsex)
                    {
                        _logger.LogError(dsex.Message);
                        return BadRequest(dsex.ToBadResult());
                    }
                }
            }

            return Ok();
        }

        [HttpDelete("{name}")]
        public IActionResult Delete(string name)
        {
            try
            {
                _botAgent.RemovePackage(WebUtility.UrlDecode(name));
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }
    }
}
