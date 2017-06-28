using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace TickTrader.DedicatedServer.Install.CmdUtil
{
    class InstallScriptBuilder
    {
        private const string _defaultInstallDir = @"$PROGRAMFILES";
        private string _appName;
        private string _installDir;
        private string _appDir;
        private string _appExe;
        private string _installer;
        private string _license;
        private string _appLnk;
        private string _brand;
        private string _serviceName;
        private string _serviceDisplayName;

        private StringBuilder nsisScript;

        public InstallScriptBuilder()
        {
            nsisScript = new StringBuilder();
        }

        public InstallScriptBuilder UseApplicationName(string appName)
        {
            _appName = appName;
            return this;
        }

        public InstallScriptBuilder UseApplicationDir(string appDir)
        {
            _appDir = appDir;
            return this;
        }

        internal InstallScriptBuilder UseServiceName(string serviceName)
        {
            _serviceName = serviceName;
            return this;
        }

        internal InstallScriptBuilder UseServiceDisplayName(string serviceDisplayName)
        {
            _serviceDisplayName = serviceDisplayName;
            return this;
        }

        public InstallScriptBuilder UseInstallDir(string installDir)
        {
            _installDir = installDir;
            return this;
        }

        public InstallScriptBuilder UseLicense(string license)
        {
            _license = license;
            return this;
        }

        public InstallScriptBuilder UseInstallerName(string name)
        {
            _installer = name;
            return this;
        }

        public InstallScriptBuilder UseApplicationExe(string exeFile)
        {
            _appExe = exeFile;
            return this;
        }

        public InstallScriptBuilder UseBrand(string brand)
        {
            _brand = brand;
            return this;
        }

        public string Build()
        {
            EnsureParams();

            Console.WriteLine("Creating installation script...");

            Console.WriteLine("Builder uses the following parameters");
            Console.WriteLine($"  AppName: {_appName}");
            Console.WriteLine($"  AppDir: {_appDir}");
            Console.WriteLine($"  AppExe: {_appExe}");
            Console.WriteLine($"  InstallDir: {_installDir}");
            Console.WriteLine($"  OutInstaller: {_installer}");
            Console.WriteLine($"  Brand: {_brand}");
            Console.WriteLine($"  ServiceName: {_serviceName}");
            Console.WriteLine($"  ServiceDisplayName: {_serviceDisplayName}");
            Console.WriteLine($"  License: {_license}");

            WriteHeader(nsisScript);
            WriteInstallSection(nsisScript);
            WriteFinishSection(nsisScript);
            IncludeInstallSection(nsisScript);
            WriteUninstallSection(nsisScript);

            if (!string.IsNullOrWhiteSpace(_brand))
                nsisScript.AppendLine($"BrandingText \"{_brand}\"");

            nsisScript.AppendLine("; eof");

            Console.WriteLine("Creating installation script - Done!");

            return nsisScript.ToString();
        }

        private void EnsureParams()
        {
            var error = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_appName))
            {
                error.AppendLine("Application name is required");
            }

            if (string.IsNullOrWhiteSpace(_appDir))
            {
                error.AppendLine("Application directory is required");
            }

            if (string.IsNullOrWhiteSpace(_appExe))
            {
                error.AppendLine("Application exe file is required");
            }

            if (error.Length != 0)
                throw new ArgumentException(error.ToString());


            if (!IsAbsolutePath(_appDir))
            {
                _appDir = $"{Directory.GetCurrentDirectory()}\\{_appDir}";
            }

            if (!IsAbsolutePath(_license))
            {
                _license = $"{Directory.GetCurrentDirectory()}\\{_license}";
            }

            if (string.IsNullOrWhiteSpace(_installDir))
            {
                _installDir = string.IsNullOrWhiteSpace(_brand) ?
                    $"{_defaultInstallDir}\\{_appName}" :
                    $"{_defaultInstallDir}\\{_brand}\\{_appName}";
            }
            else if (!IsAbsolutePath(_installDir))
            {
                _installDir = $"{_defaultInstallDir}\\{_installDir}";
            }

            if (string.IsNullOrWhiteSpace(_installer))
            {
                _installer = $"{Directory.GetCurrentDirectory()}\\{_appName} Install.exe";
            }
            else if (!IsAbsolutePath(_installer))
            {
                _installer = $"{Directory.GetCurrentDirectory()}\\{_installer}";
            }

            if (string.IsNullOrWhiteSpace(_serviceName))
                _serviceName = _appName;

            if (string.IsNullOrWhiteSpace(_serviceDisplayName))
                _serviceDisplayName = _serviceName;
        }


        private void WriteHeader(StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine("!addplugindir \"Plugins\"");
            scriptBuilder.AppendLine("; Define your application name");
            scriptBuilder.AppendLine($"!define APPNAME \"{_appName}\"");

            scriptBuilder.AppendLine("; Main Install settings");
            scriptBuilder.AppendLine($"Name \"{_appName}\"");
            scriptBuilder.AppendLine($"InstallDir \"{_installDir}\"");
            scriptBuilder.AppendLine($"InstallDirRegKey HKLM \"Software\\{_appName}\" \"\"");
            scriptBuilder.AppendLine($"OutFile \"{_installer}\"");

            scriptBuilder.AppendLine("; Modern interface settings");
            scriptBuilder.AppendLine("!include \"MUI.nsh\"");
            scriptBuilder.AppendLine("!define MUI_ABORTWARNING");
            scriptBuilder.AppendLine("!insertmacro MUI_PAGE_WELCOME");

            if (!string.IsNullOrWhiteSpace(_license))
                scriptBuilder.AppendLine($"!insertmacro MUI_PAGE_LICENSE \"{_license}\"");

            scriptBuilder.AppendLine("!insertmacro MUI_PAGE_DIRECTORY");
            scriptBuilder.AppendLine("!insertmacro MUI_PAGE_INSTFILES");
            scriptBuilder.AppendLine("!insertmacro MUI_PAGE_FINISH");

            scriptBuilder.AppendLine("!insertmacro MUI_UNPAGE_CONFIRM");
            scriptBuilder.AppendLine("!insertmacro MUI_UNPAGE_INSTFILES");

            scriptBuilder.AppendLine("; Set languages(first is default language)");
            scriptBuilder.AppendLine("!insertmacro MUI_LANGUAGE \"English\"");
            scriptBuilder.AppendLine("!insertmacro MUI_RESERVEFILE_LANGDLL");
        }

        private void WriteInstallSection(StringBuilder nsisScript)
        {
            nsisScript.AppendLine($"Section \"{_appName}\" Section1");

            nsisScript.AppendLine("\t; Set Section properties");
            nsisScript.AppendLine("\tSetOverwrite on");
            nsisScript.AppendLine("\t; Set Section Files and Shortcuts");
            nsisScript.AppendLine($"\tSetOutPath \"$INSTDIR\\\"");
            nsisScript.AppendLine($"\tFile /r \"{_appDir}\\*.*\"");
            //nsisScript.AppendLine($"\tCreateShortCut \"$DESKTOP\\{_appLnk}\" \"$INSTDIR\\{_appExe}\"");
            if (!string.IsNullOrWhiteSpace(_brand))
            {
                nsisScript.AppendLine($"\tCreateDirectory \"$SMPROGRAMS\\{_brand}\\{_appName}\"");
                nsisScript.AppendLine($"\tCreateShortCut \"$SMPROGRAMS\\{_brand}\\{_appName}\\Uninstall.lnk\" \"$INSTDIR\\uninstall.exe\"");
            }
            else
            {
                nsisScript.AppendLine($"\tCreateDirectory \"$SMPROGRAMS\\{_appName}\"");
                nsisScript.AppendLine($"\tCreateShortCut \"$SMPROGRAMS\\{_appName}\\Uninstall.lnk\" \"$INSTDIR\\uninstall.exe\"");
            }
            //nsisScript.AppendLine($"\tCreateShortCut \"$SMPROGRAMS\\{_appName}\\{_appLnk}\" \"$INSTDIR\\{_appExe}\"");

            nsisScript.AppendLine("SectionEnd");
        }

        private void WriteFinishSection(StringBuilder nsisScript)
        {
            nsisScript.AppendLine("Section - FinishSection");
            nsisScript.AppendLine($"\tWriteRegStr HKLM \"Software\\{_appName}\" \"\" \"$INSTDIR\"");
            nsisScript.AppendLine($"\tWriteRegStr HKLM \"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{_appName}\" \"DisplayName\" \"{_appName}\"");
            nsisScript.AppendLine($"\tWriteRegStr HKLM \"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{_appName}\" \"UninstallString\" \"$INSTDIR\\uninstall.exe\"");
            nsisScript.AppendLine($"\tWriteUninstaller \"$INSTDIR\\uninstall.exe\"");
            nsisScript.AppendLine("\t; Install Service");
            nsisScript.AppendLine($"\tSimpleSC::InstallService \"{_serviceName}\" \"{_serviceDisplayName}\" \"16\" \"2\" \"$INSTDIR\\{_appExe}\" \"\" \"\" \"\"");
            nsisScript.AppendLine($"\tSimpleSC::StartService \"{_serviceName}\" \"\" 0");
            nsisScript.AppendLine();
            nsisScript.AppendLine("SectionEnd");
        }

        private void IncludeInstallSection(StringBuilder nsisScript)
        {
            nsisScript.AppendLine("; Modern install component descriptions");
            nsisScript.AppendLine("!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN");
            nsisScript.AppendLine("\t!insertmacro MUI_DESCRIPTION_TEXT ${Section1} \"\"");
            nsisScript.AppendLine("!insertmacro MUI_FUNCTION_DESCRIPTION_END");
        }


        private void WriteUninstallSection(StringBuilder nsisScript)
        {
            nsisScript.AppendLine("; Uninstall section");
            nsisScript.AppendLine("Section Uninstall");

            nsisScript.AppendLine("\t; Stop and Remove Service");
            nsisScript.AppendLine($"\tSimpleSC::StopService \"{_serviceName}\" 1 80");
            nsisScript.AppendLine($"\tSimpleSC::RemoveService \"{_serviceName}\"");
            nsisScript.AppendLine("\t; Remove from registry...");
            nsisScript.AppendLine($"\tDeleteRegKey HKLM \"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{_appName}\"");
            nsisScript.AppendLine($"\tDeleteRegKey HKLM \"SOFTWARE\\{_appName}\"");
            nsisScript.AppendLine("\t; Delete self");
            nsisScript.AppendLine($"\tDelete \"$INSTDIR\\uninstall.exe\"");
            nsisScript.AppendLine("\t; Delete Shortcuts");
            //nsisScript.AppendLine($"\tDelete \"$DESKTOP\\{_appLnk}\"");
            //nsisScript.AppendLine($"\tDelete \"$SMPROGRAMS\\{_appName}\\{_appLnk}\"");

            if (!string.IsNullOrWhiteSpace(_brand))
                nsisScript.AppendLine($"\tDelete \"$SMPROGRAMS\\{_brand}\\{_appName}\\Uninstall.lnk\"");
            else
                nsisScript.AppendLine($"\tDelete \"$SMPROGRAMS\\{_appName}\\Uninstall.lnk\"");

            CleanUpInstallDir(new DirectoryInfo(_appDir), nsisScript, "$INSTDIR");

            nsisScript.AppendLine($"RMDir \"$SMPROGRAMS\\{_brand}\\{_appName}\"");
            nsisScript.AppendLine("SectionEnd");
        }



        private void CleanUpInstallDir(DirectoryInfo appDirectory, StringBuilder nsisScript, string instDirFullPath)
        {
            foreach (var file in appDirectory.GetFiles())
            {
                nsisScript.AppendLine($"\tDelete \"{instDirFullPath}\\{file.Name}\"");
            }

            foreach (var subDir in appDirectory.GetDirectories())
            {
                CleanUpInstallDir(subDir, nsisScript, $"{instDirFullPath}\\{subDir.Name}");
            }

            nsisScript.AppendLine($"\tRMDir \"{instDirFullPath}\\\"");
            nsisScript.AppendLine();
        }

        private bool IsAbsolutePath(string path)
        {
            return Path.GetFullPath(path) == path;
        }
    }
}
