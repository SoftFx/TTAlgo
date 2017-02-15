using System;

namespace TickTrader.DedicatedServer.Server.Models
{
    public class BotModel
    {
        public BotModel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}
