using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class AuthConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("servers", IsRequired = false)]
        public ServerElementCollection Servers
        {
            get { return (ServerElementCollection)this["servers"]; }
        }

        public static AuthConfigSection GetCfgSection(Configuration config = null)
        {
            if (!((config?.GetSection("auth.config") ?? ConfigurationManager.GetSection("auth.config")) is AuthConfigSection section))
                throw new ConfigurationErrorsException("Section auth.config is not found in the application configuration file.");
            return section;
        }

        public static Configuration GetConfig() => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }

    [ConfigurationCollection(typeof(ServerElement), AddItemName = "server")]
    public class ServerElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement)element).Address;
        }

        public ServerElement AddElement(string name)
        {
            LockItem = false;
            var newServer = new ServerElement() { Name = name, Address = name, Color = "#0075D8" };
            BaseAdd(newServer);
            return newServer;
        }
    }

    public class ServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("short", IsRequired = false)]
        public string ShortName
        {
            get { return (string)this["short"]; }
            set { this["short"] = value; }
        }

        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get { return (string)this["address"]; }
            set { this["address"] = value; }
        }

        [ConfigurationProperty("color", IsRequired = true)]
        public string Color
        {
            get { return (string)this["color"]; }
            set { this["color"] = value; }
        }
    }
}
