using System;
using System.IO;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.WpfWindowsSupportLibrary
{
    public class AppInstanceRestrictor : IDisposable
    {
        private FileStream _lockFile;
        private readonly string _appLockFilePath, _applicationName;

        public AppInstanceRestrictor(string appLockPath)
        {
            _applicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName;
            _appLockFilePath = appLockPath;
        }

        public bool EnsureSingleInstace()
        {
            while (!TryLock())
            {
                var lockId = TryReadProcessId();

                if (lockId != null)
                {
                    if (MessageBoxManager.YesNoBoxWarning($"Another instance of {_applicationName} is running. Terminate?"))
                    {
                        var process = System.Diagnostics.Process.GetProcessById(lockId.Value);

                        if (process != null)
                        {
                            process.Kill();

                            if (!process.WaitForExit(5000))
                            {
                                MessageBoxManager.OkError($"Failed to terminate another instance of {_applicationName}!");
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
                var processId = System.Diagnostics.Process.GetCurrentProcess().Id;

                _lockFile = new FileStream(_appLockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                _lockFile.Write(BitConverter.GetBytes(processId), 0, 4);
                _lockFile.Flush();

                return true;
            }
            catch (IOException iex)
            {
                return iex.IsLockException() ? false : throw iex;
            }
        }

        private int? TryReadProcessId()
        {
            try
            {
                using (var file = new FileStream(_appLockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var buffer = new byte[4];

                    if (file.Read(buffer, 0, 4) != 4)
                        return null;

                    return BitConverter.ToInt32(buffer, 0);
                }
            }
            catch (IOException)
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
