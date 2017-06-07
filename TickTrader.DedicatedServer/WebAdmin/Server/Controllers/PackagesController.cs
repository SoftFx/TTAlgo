using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PackagesController : Controller
    {
        private readonly ILogger<PackagesController> _logger;
        private readonly IDedicatedServer _dedicatedServer;

        public PackagesController(IDedicatedServer ddServer, ILogger<PackagesController> logger)
        {
            _dedicatedServer = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public PackageDto[] Get()
        {
            var packages = _dedicatedServer.GetPackages();

            return packages.Select(p => p.ToDto()).ToArray();
        }

        [HttpHead("{name}")]
        public IActionResult Head(string name)
        {
            var packages = _dedicatedServer.GetPackages();

            if (_dedicatedServer.GetPackages().Any(p => p.Name == name))
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
                        var pacakge = _dedicatedServer.UpdatePackage(fileContent, file.FileName);
                    }
                    catch (DSException dsex)
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
                _dedicatedServer.RemovePackage(WebUtility.UrlDecode(name));
            }
            catch (DSException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }
    }
}
