using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Abt.Licensing.Core.Credentials.SetRuntimeLicenseKey
                (@"
                <LicenseContract>
                  <Customer></Customer>
                  <OrderId>ABT151130-2635-55161</OrderId>
                  <LicenseCount>1</LicenseCount>
                  <IsTrialLicense>false</IsTrialLicense>
                  <SupportExpires>02/28/2016 00:00:00</SupportExpires>
                  <ProductCode>SC-WPF-BSC</ProductCode>
                  <KeyCode>lwAAAAEAAADQ4G3XwivRAV0AQ3VzdG9tZXI9O09yZGVySWQ9QUJUMTUxMTMwLTI2MzUtNTUxNjE7U3Vic2NyaXB0aW9uVmFsaWRUbz0yOC1GZWItMjAxNjtQcm9kdWN0Q29kZT1TQy1XUEYtQlNDEmXE14PEr431tW88nk6VzL5l93/SaEPEr2TKrxuXXUmKFXnK3mGp9RxeA0Vvaqlm</KeyCode>
                </LicenseContract>
                ");
        }
    }
}
