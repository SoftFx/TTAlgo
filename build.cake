#tool nuget:?package=vswhere&version=2.8.4
#addin nuget:?package=Newtonsoft.Json&version=13.0.1

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = ConsoleOrBuildSystemArgument("Target", "CreateAllArtifacts");
var buildNumber = ConsoleOrBuildSystemArgument("BuildNumber", "1");
var version = ConsoleOrBuildSystemArgument("Version", ""); // override only. Actual version stored in Directory.Build.props
var configuration = ConsoleOrBuildSystemArgument("Configuration", "Release");
var sourcesDir = ConsoleOrBuildSystemArgument("SourcesDir", "./");
var artifactsDirName = ConsoleOrBuildSystemArgument("ArtifactsDirName", "artifacts.build");
var details = ConsoleOrBuildSystemArgument<DotNetVerbosity>("Details", DotNetVerbosity.Normal);
var skipTests = ConsoleOrBuildSystemArgument("SkipTests", false); // used on TeamCity to enable test results integration
var nsisDirPath = ConsoleOrBuildSystemArgument("NsisPath", @"c:/Program Files (x86)/NSIS/");
var msBuildDirPath = ConsoleOrBuildSystemArgument("MSBuildPath", "");
var useGithubBuild = ConsoleOrBuildSystemArgument("UseGithubBuild", false);

var sourcesDirPath = DirectoryPath.FromString(sourcesDir);
var buildId = "1.0.0.0"; // stub
var artifactsPath = sourcesDirPath.Combine(artifactsDirName);
var mainSolutionPath = sourcesDirPath.CombineWithFilePath("Algo.sln");
var sdkSolutionPath = sourcesDirPath.CombineWithFilePath("src/csharp/TickTrader.Algo.Sdk.sln");
var nsisPath = DirectoryPath.FromString(nsisDirPath).CombineWithFilePath("makensis.exe");
var setupDirPath = sourcesDirPath.Combine("setup");

var isGithubBuild = useGithubBuild || BuildSystem.IsRunningOnGitHubActions;

var outputPath = sourcesDirPath.Combine("bin");
var terminalProjectPath = sourcesDirPath.CombineWithFilePath("TickTrader.BotTerminal/TickTrader.BotTerminal.csproj");
var terminalBinPath = outputPath.Combine("terminal");
var serverProjectPath = sourcesDirPath.CombineWithFilePath("TickTrader.BotAgent/TickTrader.BotAgent.csproj");
var serverBinPath = outputPath.Combine("server");
var configuratorProjectPath = sourcesDirPath.CombineWithFilePath("TickTrader.BotAgent.Configurator/TickTrader.BotAgent.Configurator.csproj");
var configuratorBinPath = outputPath.Combine("configurator");
var publicApiProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/core/TickTrader.Algo.Server.PublicAPI.Client/TickTrader.Algo.Server.PublicAPI.Client.csproj");
var publicApiBinPath = outputPath.Combine("public-api");
var symbolStorageProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/core/TickTrader.FeedStorage/TickTrader.FeedStorage.csproj");
var symbolStorageBinPath = outputPath.Combine("symbol-storage");
var backtesterApiProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/core/TickTrader.Algo.BacktesterApi/TickTrader.Algo.BacktesterApi.csproj");
var backtesterApiBinPath = outputPath.Combine("backtester-api");
var backtesterHostProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/apps/TickTrader.Algo.BacktesterV1Host/TickTrader.Algo.BacktesterV1Host.csproj");
var backtesterHostBinPath = outputPath.Combine("backtester-host");
var runtimeHostProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/apps/TickTrader.Algo.RuntimeV1Host/TickTrader.Algo.RuntimeV1Host.csproj");
var runtimeHostBinPath = outputPath.Combine("runtime-host");
var pkgLoaderProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/core/TickTrader.Algo.PkgLoader/TickTrader.Algo.PkgLoader.csproj");
var pkgLoaderBinPath = outputPath.Combine("pkg-loader");
var indicatorHostProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/core/TickTrader.Algo.IndicatorHost/TickTrader.Algo.IndicatorHost.csproj");
var indicatorHostBinPath = outputPath.Combine("indicator-host");
var vsExtensionPath = sourcesDirPath.CombineWithFilePath($"src/csharp/sdk/TickTrader.Algo.VS.Package/bin/{configuration}/TickTrader.Algo.VS.Package.vsix");
var updaterProjectPath = sourcesDirPath.CombineWithFilePath("src/csharp/apps/TickTrader.Algo.Updater/TickTrader.Algo.Updater.csproj");
var updaterBinPath = outputPath.Combine("updater");
var terminalUpdateBinPath = outputPath.Combine("terminal-update");
var serverUpdateBinPath = outputPath.Combine("server-update");
var updateInfoOutputPath = outputPath.CombineWithFilePath("update-info.json");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   var exitCode = StartProcess("dotnet", new ProcessSettings {
      WorkingDirectory = sourcesDir,
      Arguments = "--info"
    });

   if (exitCode != 0)
      throw new Exception($"Failed to get .NET SDK info: {exitCode}");

   if (string.IsNullOrEmpty(msBuildDirPath))
   {
      var paths = VSWhereAll(new VSWhereAllSettings{ Requires="Microsoft.VisualStudio.Workload.VisualStudioExtension" });
      Information("Found {0} vs installation that have VisualStudioExtension workload", paths.Count);
      foreach (var path in paths)
      {
         Information("\t{0}", path);
      }
   }
});

TaskSetup(ctx =>
{
   if (BuildSystem.IsRunningOnGitHubActions)
      GitHubActions.Commands.StartGroup(ctx.Task.Name);
   else if (BuildSystem.IsRunningOnTeamCity)
      TeamCity.WriteStartBlock(ctx.Task.Name);
});

TaskTeardown(ctx =>
{
   if (BuildSystem.IsRunningOnGitHubActions)
      GitHubActions.Commands.EndGroup();
   else if (BuildSystem.IsRunningOnTeamCity)
      TeamCity.WriteEndBlock(ctx.Task.Name);
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
   .Does(() =>
{
   DotNetClean(mainSolutionPath.ToString(), new DotNetCleanSettings {
      Configuration = configuration,
      Verbosity = details,
   });
   CleanDirectory(outputPath);
   CleanDirectory(artifactsPath);
});

Task("BuildMainProject")
   .IsDependentOn("Clean")
   .Does(() =>
{
   var msBuildSettings = GetMSBuildSettingsWithVersionProps()
      .WithProperty("AlgoPackage_OutputPath", artifactsPath.MakeAbsolute(Context.Environment).ToString());

   DotNetBuild(mainSolutionPath.ToString(), new DotNetBuildSettings {
      Configuration = configuration,
      Verbosity = details,
      MSBuildSettings = msBuildSettings,
   });
});

Task("BuildSdk")
   .IsDependentOn("Clean")
   .Does(() =>
{
   DotNetRestore(sdkSolutionPath.ToString());

   var msBuildPath = DirectoryPath.FromString(msBuildDirPath).CombineWithFilePath("MSBuild.exe").ToString();
   if (!System.IO.File.Exists(msBuildPath))
   {
      Information("Looking for MSBuild with VS extension SDK. File '{0}' doesn't exists", msBuildPath);

      var vsInstallPath = VSWhereLatest(new VSWhereLatestSettings{ Requires = "Microsoft.VisualStudio.Workload.VisualStudioExtension" });
      msBuildPath = GetFiles(vsInstallPath.CombineWithFilePath("MSBuild/**/Bin/MSBuild.exe").ToString()).FirstOrDefault()?.ToString();
      if (string.IsNullOrEmpty(msBuildPath))
         throw new Exception("Failed to resolve MSBuild with VS extension sdk");

      Information("Found MSBuild at '{0}'", msBuildPath);
   }

   var msBuildSettings = new MSBuildSettings {
      ToolPath = msBuildPath,
      Configuration = configuration,
      Verbosity = Verbosity.Normal,
   };

   MSBuild(sdkSolutionPath, msBuildSettings);

   if (!isGithubBuild)
      CopyFiles(vsExtensionPath.FullPath, artifactsPath);
});

Task("Test")
   .IsDependentOn("BuildMainProject")
   .Does(() =>
{
   if (skipTests)
   {
      Information("Test were skipped intentionally");
      return;
   }

   var testProjects = GetFiles(sourcesDirPath.Combine("src/csharp/tests/**/*.csproj").ToString());
   foreach(var testProj in testProjects)
   {
      DotNetTest(testProj.ToString(), new DotNetTestSettings {
         Configuration = configuration,
         Verbosity = details,
         NoBuild = true,
      });
   }
});

Task("BuildAndTest")
   .IsDependentOn("BuildMainProject")
   .IsDependentOn("BuildSdk")
   .IsDependentOn("Test");

Task("PublishTerminal")
   .IsDependentOn("BuildAndTest")
   .Does(() =>
{
   // we need to change post-build tasks to work with publish
   DotNetBuild(terminalProjectPath.FullPath, new DotNetBuildSettings {
      Configuration = configuration,
      Verbosity = details,
      NoRestore = true,
      OutputDirectory = terminalBinPath,
      MSBuildSettings = GetMSBuildSettingsWithVersionProps(),
   });

   var redistPath = terminalBinPath.Combine("Redist");
   CreateDirectory(redistPath);
   CopyFiles(vsExtensionPath.FullPath, redistPath);

   if (!isGithubBuild)
   {
      // NewsIndicator is not working now
      // var repoPath = terminalBinPath.Combine("AlgoRepository");
      // CreateDirectory(repoPath);
      // CopyFiles(artifactsPath.CombineWithFilePath("TickTrader.Algo.NewsIndicator.ttalgo").FullPath, repoPath);
   }
});

Task("PublishConfigurator")
   .IsDependentOn("BuildAndTest")
   .Does(() => DotNetPublish(configuratorProjectPath.FullPath, GetPublishSettings(configuratorBinPath)));

Task("PublishServer")
   .IsDependentOn("BuildAndTest")
   .IsDependentOn("PublishConfigurator")
   .Does(() =>
{
   DotNetPublish(serverProjectPath.FullPath, new DotNetPublishSettings {
      Configuration = configuration,
      Verbosity = details,
      OutputDirectory = serverBinPath,
      MSBuildSettings = GetMSBuildSettingsWithVersionProps(),
   });

   var configuratorInstallPath = serverBinPath.Combine("Configurator");
   CopyDirectory(configuratorBinPath, configuratorInstallPath);
});

Task("PublishUpdater")
   .IsDependentOn("BuildAndTest")
   .Does(() => DotNetPublish(updaterProjectPath.FullPath, GetPublishSettings(updaterBinPath)));

Task("PublishMainProjects")
   .IsDependentOn("PublishTerminal")
   .IsDependentOn("PublishConfigurator")
   .IsDependentOn("PublishServer")
   .IsDependentOn("PublishUpdater")
   .Does(() =>
{
   var serverDllPath = serverBinPath.CombineWithFilePath("TickTrader.Algo.Server.dll").FullPath;
   var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(serverDllPath).FileVersion;
   Information($"Loaded version '{fileVersion}' from '{serverDllPath}'");
   if (fileVersion == null || fileVersion.EndsWith(".0.0") || fileVersion.EndsWith(".0.0.0"))
      throw new Exception($"Bad file version: '{fileVersion}'");
   buildId = fileVersion;

   if (isGithubBuild)
      GitHubActions.Commands.SetOutputParameter("version", buildId);
});

Task("PublishPublicApi")
   .IsDependentOn("PublishMainProjects")
   .WithCriteria(!isGithubBuild)
   .Does(() =>
{
   DotNetPublish(publicApiProjectPath.FullPath, GetPublishSettings(publicApiBinPath.Combine("net472"), "net472"));

   DeleteFiles(publicApiBinPath.Combine("net472").CombineWithFilePath("libgrpc_csharp_ext*").ToString());

   DotNetPublish(publicApiProjectPath.FullPath, GetPublishSettings(publicApiBinPath.Combine("net6.0"), "net6.0"));
});

Task("PublishSymbolStorage")
   .IsDependentOn("PublishMainProjects")
   .WithCriteria(!isGithubBuild)
   .Does(() => DotNetPublish(symbolStorageProjectPath.FullPath, GetPublishSettings(symbolStorageBinPath)));

Task("PublishBacktesterApi")
   .IsDependentOn("PublishMainProjects")
   .WithCriteria(!isGithubBuild)
   .Does(() => DotNetPublish(backtesterApiProjectPath.FullPath, GetPublishSettings(backtesterApiBinPath)));

Task("PublishBacktesterHost")
   .IsDependentOn("PublishMainProjects")
   .WithCriteria(!isGithubBuild)
   .Does(() => DotNetPublish(backtesterHostProjectPath.FullPath, GetPublishSettings(backtesterHostBinPath)));

Task("PublishRuntimeHost")
   .IsDependentOn("PublishMainProjects")
   .WithCriteria(!isGithubBuild)
   .Does(() => DotNetPublish(runtimeHostProjectPath.FullPath, GetPublishSettings(runtimeHostBinPath)));

Task("PublishPkgLoader")
   .IsDependentOn("PublishMainProjects")
   .WithCriteria(!isGithubBuild)
   .Does(() => DotNetPublish(pkgLoaderProjectPath.FullPath, GetPublishSettings(pkgLoaderBinPath, "net6.0")));

Task("PublishIndicatorHost")
   .IsDependentOn("PublishMainProjects")
   .IsDependentOn("PublishPkgLoader")
   .IsDependentOn("PublishRuntimeHost")
   .WithCriteria(!isGithubBuild)
   .Does(() =>
{
   DotNetPublish(indicatorHostProjectPath.FullPath, GetPublishSettings(indicatorHostBinPath));

   CopyDirectory(pkgLoaderBinPath.FullPath, indicatorHostBinPath);

   var indicatorHostRuntimePath = indicatorHostBinPath.Combine("bin").Combine("runtime");
   CopyDirectory(runtimeHostBinPath, indicatorHostRuntimePath);
});

Task("PublishDevProjects")
   .IsDependentOn("PublishPublicApi")
   .IsDependentOn("PublishSymbolStorage")
   .IsDependentOn("PublishBacktesterApi")
   .IsDependentOn("PublishBacktesterHost")
   .IsDependentOn("PublishIndicatorHost")
   .WithCriteria(!isGithubBuild);

Task("PublishAllProjects")
   .IsDependentOn("PublishMainProjects")
   .IsDependentOn("PublishDevProjects");

Task("CreateUpdate")
   .IsDependentOn("PublishMainProjects")
   .Does(() =>
{
   CreateUpdateInfo();
   CopyFileToDirectory(updateInfoOutputPath, artifactsPath);

   CopyDirectory(updaterBinPath, terminalUpdateBinPath);
   CopyDirectory(terminalBinPath, terminalUpdateBinPath.Combine("update"));
   CopyFileToDirectory(updateInfoOutputPath, terminalUpdateBinPath);
   Zip(terminalUpdateBinPath, artifactsPath.CombineWithFilePath($"AlgoTerminal {buildId}.x64.Update.zip"));

   CopyDirectory(updaterBinPath, serverUpdateBinPath);
   CopyDirectory(serverBinPath, serverUpdateBinPath.Combine("update"));
   CopyFileToDirectory(updateInfoOutputPath, serverUpdateBinPath);
   Zip(serverUpdateBinPath, artifactsPath.CombineWithFilePath($"AlgoServer {buildId}.x64.Update.zip"));
});

Task("ZipArtifacts")
   .IsDependentOn("PublishAllProjects")
   .WithCriteria(!isGithubBuild)
   .Does(() =>
{
   Zip(terminalBinPath, artifactsPath.CombineWithFilePath($"AlgoTerminal {buildId}.x64.zip"));
   Zip(serverBinPath, artifactsPath.CombineWithFilePath($"AlgoServer {buildId}.x64.zip"));
   Zip(configuratorBinPath, artifactsPath.CombineWithFilePath($"AlgoServer Configurator {buildId}.x64.zip"));
   Zip(publicApiBinPath, artifactsPath.CombineWithFilePath($"PublicAPI {buildId}.zip"));
   Zip(symbolStorageBinPath, artifactsPath.CombineWithFilePath($"SymbolStorage {buildId}.x64.zip"));
   Zip(backtesterApiBinPath, artifactsPath.CombineWithFilePath($"BacktesterApi {buildId}.zip"));
   Zip(backtesterHostBinPath, artifactsPath.CombineWithFilePath($"BacktesterV1Host {buildId}.x64.zip"));
   Zip(indicatorHostBinPath, artifactsPath.CombineWithFilePath($"IndicatorHost {buildId}.zip"));
});

Task("CreateInstaller")
   .IsDependentOn("PublishMainProjects")
   .Does(() =>
{
   if (!System.IO.File.Exists(nsisPath.MakeAbsolute(Context.Environment).ToString()))
   {
      if (BuildSystem.IsLocalBuild)
         Error("Failed to create installer: NSIS not found!");
      else
         throw new Exception("Failed to create installer: NSIS not found!");
   }

   CreateUninstallScript(terminalBinPath, setupDirPath.CombineWithFilePath("Terminal.Uninstall.nsi"));
   CreateUninstallScript(serverBinPath, setupDirPath.CombineWithFilePath("AlgoServer.Uninstall.nsi"));
   CreateUninstallScript(configuratorBinPath, setupDirPath.CombineWithFilePath("Configurator.Uninstall.nsi"));

   StartProcess(nsisPath, new ProcessSettings {
      Arguments = $"/DPRODUCT_BUILD={buildId} /DOUTPUT_DIR=../{artifactsDirName} {setupDirPath.CombineWithFilePath("Algo.Setup.nsi").FullPath}",
   });
});

Task("CreateAllArtifacts")
   .IsDependentOn("ZipArtifacts")
   .IsDependentOn("CreateUpdate")
   .IsDependentOn("CreateInstaller");

PrintArguments();
RunTarget(target);

public void PrintArguments()
{
   Information("Target: {0}", target);
   Information("BuildNumber: {0}", buildNumber);
   Information("Version: {0}", version);
   Information("Configuration: {0}", configuration);
   Information("SourcesDir: {0}", sourcesDir);
   Information("ArtifactsDirName: {0}", artifactsDirName);
   Information("Details: {0}", details);
   Information("SkipTests: {0}", skipTests);
   Information("NsisPath: {0}", nsisDirPath);
   Information("IsGithubBuild: {0}", isGithubBuild);
}

public string ConsoleOrBuildSystemArgument(string name, string defautValue) => ConsoleOrBuildSystemArgument<string>(name, defautValue);

public T ConsoleOrBuildSystemArgument<T>(string name, T defautValue)
{
    if (HasArgument(name))
        return Argument<T>(name);

    if (BuildSystem.IsRunningOnTeamCity
        && TeamCity.Environment.Build.BuildProperties.TryGetValue(name, out var teamCityProperty))
    {
        Information("Found Teamcity property: {0}", name);

        const string envVarName = "env_TempTeamCityProperty";
        Environment.SetEnvironmentVariable(envVarName, teamCityProperty, EnvironmentVariableTarget.Process);
        return EnvironmentVariable<T>(envVarName, defautValue);
    }

    return defautValue;
}

public void CreateUninstallScript(DirectoryPath appDir, FilePath scriptPath)
{
   var sb = new StringBuilder();
   CleanUpInstallDir(new DirectoryInfo(appDir.FullPath), sb, "$INSTDIR");
   System.IO.File.WriteAllText(scriptPath.FullPath, sb.ToString());
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

private DotNetMSBuildSettings GetMSBuildSettingsWithVersionProps()
{
   var settings = new DotNetMSBuildSettings();
   if (!string.IsNullOrEmpty(version))
      settings.WithProperty("BuildVersion", version.ToString());
   if (!string.IsNullOrEmpty(buildNumber))
      settings.WithProperty("BuildNumber", buildNumber.ToString());
   return settings;
}

private DotNetPublishSettings GetPublishSettings(DirectoryPath outputDir)
{  
   return new DotNetPublishSettings {
      Configuration = configuration,
      Verbosity = details,
      NoBuild = true,
      OutputDirectory = outputDir,
   };
}

private DotNetPublishSettings GetPublishSettings(DirectoryPath outputDir, string framework)
{
   return new DotNetPublishSettings {
      Configuration = configuration,
      Verbosity = details,
      NoBuild = true,
      OutputDirectory = outputDir,
      Framework = framework,
   };
}

private void CreateUpdateInfo()
{
   var changelogText = System.IO.File.ReadAllText(sourcesDirPath.CombineWithFilePath("ReleaseNote.md").ToString());
   var updateInfo = new UpdateInfo {
      ReleaseVersion = buildId,
      ReleaseDate = System.DateTime.UtcNow.ToString("yyyy.MM.dd"),
      MinVersion = "1.24.0.0",
      Executable = "TickTrader.Algo.Updater.exe",
      Changelog = changelogText,
   };
   SerializeJsonToPrettyFile(updateInfoOutputPath, updateInfo);
}

private void SerializeJsonToPrettyFile<T>(FilePath filePath, T dataObj)
{
   var json = Newtonsoft.Json.JsonConvert.SerializeObject(dataObj, Newtonsoft.Json.Formatting.Indented);
   System.IO.File.WriteAllText(filePath.MakeAbsolute(Context.Environment).ToString(), json);
}

private class UpdateInfo
{
   public string ReleaseVersion { get; set; }

   public string ReleaseDate { get; set; }

   public string MinVersion { get; set; }

   public string Executable { get; set; }

   public string Changelog { get; set; }
}