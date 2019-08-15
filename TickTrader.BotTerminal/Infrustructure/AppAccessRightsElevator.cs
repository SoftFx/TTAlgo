using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;

namespace TickTrader.BotTerminal
{
    public enum AccessElevationStatus
    {
        Launched = 0,
        UserCancelled = 1,
        AlreadyThere = 2,
    }

    public static class AppAccessRightsElevator
    {
        public static AccessElevationStatus ElevateToAdminRights()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                return AccessElevationStatus.AlreadyThere;
            }
            else
            {
                var startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                };
                try
                {
                    Process.Start(startInfo);
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 1223) // user canceled UAC prompt
                    {
                        return AccessElevationStatus.UserCancelled;
                    }
                    throw;
                }
                return AccessElevationStatus.Launched;
            }
        }
    }
}
