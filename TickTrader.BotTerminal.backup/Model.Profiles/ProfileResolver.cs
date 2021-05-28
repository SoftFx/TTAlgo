using NLog;
//using Ver1 = TickTrader.BotTerminal.Model.Profiles.Version1;

namespace TickTrader.BotTerminal
{
    internal static partial class ProfileResolver
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();


        public static bool TryResolveProfile(string filePath)
        {
            if (TryResolveVersion1(filePath))
                return true;
            return false;
        }


        private static bool TryResolveVersion1(string filePath)
        {
            return false;

//            try
//            {
//                Ver1.ProfileStorageModel legacyProfile;
//                using (var file = File.OpenRead(filePath))
//                {
//                    var serializer = new DataContractSerializer(typeof(Ver1.ProfileStorageModel));
//                    legacyProfile = (Ver1.ProfileStorageModel)serializer.ReadObject(file);
//                }

//                var profile = ResolveProfileVersion1(legacyProfile);

//                using (var file = File.OpenWrite(filePath))
//                {
//                    var serializer = new DataContractSerializer(typeof(ProfileStorageModel));
//#if DEBUG
//                    using (var xmlWriter = XmlWriter.Create(file, new XmlWriterSettings { Indent = true }))
//                    {
//                        serializer.WriteObject(xmlWriter, profile);
//                    }
//#else
//                    serializer.WriteObject(file, profile);
//#endif
//                }

//                _logger.Info($"Successfully resolved current profile at {filePath} to version 1");
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.Info($"Can't resolve current profile at {filePath} to version 1: {ex.Message}");
//                return false;
//            }
        }
    }
}
