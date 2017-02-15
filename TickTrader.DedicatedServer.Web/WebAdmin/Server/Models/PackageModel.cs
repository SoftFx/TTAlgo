using System;

namespace TickTrader.DedicatedServer.Server.Models
{
    public class PackageModel
    {
        public PackageModel()
        {

        }

        public PackageModel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public PackageModel(string name, BotModel[] bots):this(name)
        {
            Bots = bots;
        }

        public string Name { get; set; }
        public BotModel[] Bots { get; set; }
    }
}
