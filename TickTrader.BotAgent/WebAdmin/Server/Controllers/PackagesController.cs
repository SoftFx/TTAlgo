using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PackagesController : Controller
    {
        private readonly ILogger<PackagesController> _logger;
        private readonly IAlgoServerLocal _algoServer;

        public PackagesController(IAlgoServerLocal algoServer, ILogger<PackagesController> logger)
        {
            _algoServer = algoServer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<PackageDto[]> Get()
        {
            var snapshot = await _algoServer.GetPackageSnapshot();

            return snapshot.Packages.Select(p => p.ToDto()).ToArray();
        }

        [HttpHead("{pkgName}")]
        public async Task<IActionResult> Head(string pkgName)
        {
            if (await _algoServer.PackageWithNameExists(pkgName))
                return Ok();

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException("File is null");
            if (file.Length == 0) throw new ArgumentException("File is empty");

            try
            {
                var tmpFile = Path.GetTempFileName();
                using (var fileStream = System.IO.File.OpenWrite(tmpFile))
                {
                    await file.CopyToAsync(fileStream);
                }

                await _algoServer.UploadPackage(new UploadPackageRequest(null, file.FileName), tmpFile);

                if (System.IO.File.Exists(tmpFile))
                    System.IO.File.Delete(tmpFile);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Package upload failed");
            }

            return Ok();
        }

        [HttpDelete("{packageId}")]
        public async Task<IActionResult> Delete(string packageId)
        {
            try
            {
                await _algoServer.RemovePackage(new RemovePackageRequest(WebUtility.UrlDecode(packageId)));
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }

            return Ok();
        }
    }
}
