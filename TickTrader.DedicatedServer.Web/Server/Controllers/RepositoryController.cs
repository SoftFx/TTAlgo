using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.DedicatedServer.Server.DS;
using TickTrader.DedicatedServer.Server.Models;

namespace TickTrader.DedicatedServer.Server.Controllers
{
    [Route("api/[controller]")]
    public class RepositoryController: Controller
    {
        private IDedicatedServer _dedicatedServer;

        public RepositoryController(IDedicatedServer ddServer)
        {
            this._dedicatedServer = ddServer;
        }

        [HttpGet]
        public async Task<PackageModel[]> Get()
        {
            return await _dedicatedServer.GetPackagesAsync();
        }

        [HttpPost]
        public async Task Post(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException("File is null");
            if (file.Length == 0) throw new ArgumentException("File is empty");

            using (var stream = file.OpenReadStream())
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    var fileContent = binaryReader.ReadBytes((int)file.Length);
                    await _dedicatedServer.AddPackageAsync(fileContent, file.FileName);
                }
            }
        }

        [HttpDelete("{package}")]
        public async Task Delete(string package)
        {
            await _dedicatedServer.RemovePackageAsync(package);
        }
    }
}
