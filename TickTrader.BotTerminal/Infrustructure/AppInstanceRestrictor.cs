using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    public class AppInstanceRestrictor : IDisposable
    {
        private FileStream _lockFile;

        public bool EnsureSingleInstace()
        {
            while (!TryLock())
            {
                var lockId = TryReadProcessId();
                if (lockId != null)
                {
                    var result = MessageBox.Show("Another instance of BotTerminal is running. Terminate?", "Error", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var process = System.Diagnostics.Process.GetProcessById(lockId.Value);
                        if (process != null)
                        {
                            process.Kill();
                            if (!process.WaitForExit(5000))
                            {
                                MessageBox.Show("Failed to terminate another instance of BotTerminal!", "Error", MessageBoxButton.OK);
                                return false;
                            }
                            else continue; // go for another try
                        }
                    }
                }
                return false;
            }

            return true;
        }

        private bool TryLock()
        {
            try
            {
                _lockFile = new FileStream(EnvService.Instance.AppLockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                var processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                _lockFile.Write(BitConverter.GetBytes(processId), 0, 4);
                _lockFile.Flush();
                return true;
            }
            catch (IOException iex)
            {
                if (iex.IsLockExcpetion())
                    return false;
                throw;
            }
        }

        private int? TryReadProcessId()
        {
            try
            {
                using (var file = new FileStream(EnvService.Instance.AppLockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var buffer = new byte[4];

                    if (file.Read(buffer, 0, 4) != 4)
                        return null;

                    return BitConverter.ToInt32(buffer, 0);
                }
            }
            catch (IOException iex)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_lockFile != null)
                _lockFile.Close();
        }
    }
}
