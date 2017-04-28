using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RepositoryController : Controller
    {
        private readonly ILogger<RepositoryController> _logger;
        private readonly IDedicatedServer _dedicatedServer;

        public RepositoryController(IDedicatedServer ddServer, ILogger<RepositoryController> logger)
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
                        var pacakge = _dedicatedServer.AddPackage(fileContent, file.FileName);
                    }
                    catch (DuplicatePackageException dpe)
                    {
                        _logger.LogError(dpe.Message);
                        return BadRequest(new { Code = dpe.Code, Message = dpe.Message });
                    }
                }
            }

            return Ok();
        }

        [HttpDelete("{name}")]
        public void Delete(string name)
        {
            _dedicatedServer.RemovePackage(name);
        }
    }
}
