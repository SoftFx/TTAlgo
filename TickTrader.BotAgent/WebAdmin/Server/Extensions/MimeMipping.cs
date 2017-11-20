using Microsoft.AspNetCore.StaticFiles;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class MimeMipping
    {
        public static string GetContentType(string fileName)
        {
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
            return contentType ?? "application/octet-stream";
        }
    }
}
