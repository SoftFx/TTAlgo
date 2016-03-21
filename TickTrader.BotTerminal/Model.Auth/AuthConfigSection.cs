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

        public static AuthConfigSection GetCfgSection()
        {
            AuthConfigSection section = ConfigurationManager.GetSection("auth.config") as AuthConfigSection;
            if (section == null)
                throw new ConfigurationErrorsException("Section auth.config is not found in the application configuration file.");
            return section;
        }
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
    }

    public class ServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        [ConfigurationProperty("short", IsRequired = false)]
        public string ShortName
        {
            get { return (string)this["short"]; }
        }

        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get { return (string)this["address"]; }
        }

        [ConfigurationProperty("color", IsRequired = true)]
        public string Color
        {
            get { return (string)this["color"]; }
        }
    }
}
