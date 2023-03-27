#tool nuget:?package=vswhere&version=2.8.4

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = ConsoleOrBuildSystemArgument("Target", "ZipGithubArtifacts");
var buildNumber = ConsoleOrBuildSystemArgument("BuildNumber", 0);
var version = ConsoleOrBuildSystemArgument("Version", "1.19");
var configuration = ConsoleOrBuildSystemArgument("Configuration", "Release");
var sourcesDir = ConsoleOrBuildSystemArgument("SourcesDir", "./");
var artifactsDirName = ConsoleOrBuildSystemArgument("ArtifactsDirName", "artifacts.build");
var details = ConsoleOrBuildSystemArgument<DotNetVerbosity>("Details", DotNetVerbosity.Normal);
var skipTests = ConsoleOrBuildSystemArgument("SkipTests", false); // used on TeamCity to enable test results integration
var nsisDirPath = ConsoleOrBuildSystemArgument("NsisPath", @"c:/Program Files (x86)/NSIS/");
var msBuildDirPath = ConsoleOrBuildSystemArgument("MSBuildPath", "");

var sourcesDirPath = DirectoryPath.FromString(sourcesDir);
var buildId = $"{version}.{buildNumber}.0";
var artifactsPath = sourcesDirPath.Combine(artifactsDirName);
var mainSolutionPath = sourcesDirPath.CombineWithFilePath("Algo.sln");
var sdkSolutionPath = sourcesDirPath.CombineWithFilePath("src/csharp/TickTrader.Algo.Sdk.sln");
var nsisPath = DirectoryPath.FromString(nsisDirPath).CombineWithFilePath("makensis.exe");
var setupDirPath = sourcesDirPath.Combine("setup");

var isGithubBuild = BuildSystem.IsRunningOnGitHubActions;

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

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("Clean") : null;

   try
   {
      DotNetClean(mainSolutionPath.ToString(), new DotNetCleanSettings {
         Configuration = configuration,
         Verbosity = details,
      });
      CleanDirectory(outputPath);
      CleanDirectory(artifactsPath);
   }
   finally
   {
      block?.Dispose();
   }
});

Task("BuildMainProject")
   .IsDependentOn("Clean")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("BuildMainProject") : null;

   try
   {
      var msBuildSettings = new DotNetMSBuildSettings();
      msBuildSettings.WithProperty("AlgoPackage_OutputPath", artifactsPath.MakeAbsolute(Context.Environment).ToString());

      DotNetBuild(mainSolutionPath.ToString(), new DotNetBuildSettings {
         Configuration = configuration,
         Verbosity = details,
         MSBuildSettings = msBuildSettings,
      });
   }
   finally
   {
      block?.Dispose();
   }
});

Task("BuildSdk")
   .IsDependentOn("Clean")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("BuildSdk") : null;

   try
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
   }
   finally
   {
      block?.Dispose();
   }
});

Task("BuildAll")
   .IsDependentOn("BuildMainProject")
   .IsDependentOn("BuildSdk");

Task("Test")
   .IsDependentOn("BuildAll")
   .Does(() =>
{
   if (skipTests)
   {
      Information("Test were skipped intentionally");
      return;
   }

   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("Test") : null;

   try
   {
      var testProjects = GetFiles(sourcesDirPath.Combine("src/csharp/tests/**/*.csproj").ToString());
      foreach(var testProj in testProjects)
      {
         DotNetTest(testProj.ToString(), new DotNetTestSettings {
            Configuration = configuration,
            Verbosity = details,
         });
      }
   }
   finally
   {
      block?.Dispose();
   }
});

Task("PublishTerminal")
   .IsDependentOn("Test")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishTerminal") : null;

   try
   {
      // we need to change post-build tasks to work with publish
      DotNetBuild(terminalProjectPath.FullPath, new DotNetBuildSettings {
         Configuration = configuration,
         Verbosity = details,
         NoRestore = true,
         OutputDirectory = terminalBinPath,
      });
   }
   finally
   {
      block?.Dispose();
   }
});

Task("PublishConfigurator")
   .IsDependentOn("Test")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishConfigurator") : null;

   try
   {
      DotNetPublish(configuratorProjectPath.FullPath, new DotNetPublishSettings {
         Configuration = configuration,
         Verbosity = details,
         NoBuild = true,
         OutputDirectory = configuratorBinPath,
      });
   }
   finally
   {
      block?.Dispose();
   }
});

Task("PublishServer")
   .IsDependentOn("Test")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishServer") : null;

   try
   {
      DotNetPublish(serverProjectPath.FullPath, new DotNetPublishSettings {
         Configuration = configuration,
         Verbosity = details,
         OutputDirectory = serverBinPath,
      });
   }
   finally
   {
      block?.Dispose();
   }
});

Task("PublishGithubProjects")
   .IsDependentOn("Test")
   .IsDependentOn("PublishTerminal")
   .IsDependentOn("PublishConfigurator")
   .IsDependentOn("PublishServer")
   .WithCriteria(isGithubBuild);

// Task("PublishTeamCityProjects")
//    .IsDependentOn("Test")
//    .IsDependentOn("PublishTerminal")
//    .IsDependentOn("PublishConfigurator")
//    .IsDependentOn("PublishServer")
//    .WithCriteria(!isGithubBuild);

// Task("PublishPublicApi")
//    .IsDependentOn("PublishTeamCityProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishPublicApi") : null;

//    try
//    {
//       DotNetPublish(publicApiProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = publicApiBinPath.Combine("net472"),
//          Framework = "net472"
//       });

//       DeleteFiles(publicApiBinPath.Combine("net472").CombineWithFilePath("libgrpc_csharp_ext*").ToString());

//       DotNetPublish(publicApiProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = publicApiBinPath.Combine("net6.0"),
//          Framework = "net6.0"
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("PublishSymbolStorage")
//    .IsDependentOn("PublishTeamCityProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishSymbolStorage") : null;

//    try
//    {
//       DotNetPublish(symbolStorageProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = symbolStorageBinPath,
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("PublishBacktesterApi")
//    .IsDependentOn("PublishTeamCityProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishBacktesterApi") : null;

//    try
//    {
//       DotNetPublish(backtesterApiProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = backtesterApiBinPath
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("PublishBacktesterHost")
//    .IsDependentOn("PublishTeamCityProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishBacktesterHost") : null;

//    try
//    {
//       DotNetPublish(backtesterHostProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = backtesterHostBinPath
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("PublishRuntimeHost")
//    .IsDependentOn("PublishTeamCityProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishRuntimeHost") : null;

//    try
//    {
//       DotNetPublish(runtimeHostProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = runtimeHostBinPath,
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("PublishPkgLoader")
//    .IsDependentOn("PublishTeamCityProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishPkgLoader") : null;

//    try
//    {
//       DotNetPublish(pkgLoaderProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = pkgLoaderBinPath,
//          Framework = "net6.0",
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("PublishIndicatorHost")
//    .IsDependentOn("PublishTeamCityProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishIndicatorHost") : null;

//    try
//    {
//       DotNetPublish(indicatorHostProjectPath.FullPath, new DotNetPublishSettings {
//          Configuration = configuration,
//          Verbosity = details,
//          NoBuild = true,
//          OutputDirectory = indicatorHostBinPath,
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("PublishAllProjects")
//    .IsDependentOn("PublishTerminal")
//    .IsDependentOn("PublishConfigurator")
//    .IsDependentOn("PublishServer")
//    .IsDependentOn("PublishPublicApi")
//    .IsDependentOn("PublishSymbolStorage")
//    .IsDependentOn("PublishBacktesterApi")
//    .IsDependentOn("PublishBacktesterHost")
//    .IsDependentOn("PublishRuntimeHost")
//    .IsDependentOn("PublishPkgLoader")
//    .IsDependentOn("PublishIndicatorHost");

// Task("PrepareArtifacts")
//    .IsDependentOn("PublishAllProjects")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PrepareArtifacts") : null;

//    try
//    {
//       var redistPath = terminalBinPath.Combine("Redist");
//       var repoPath = terminalBinPath.Combine("AlgoRepository");

//       CreateDirectory(redistPath);
//       CreateDirectory(repoPath);
//       CopyFiles(vsExtensionPath.FullPath, redistPath);
//       CopyFiles(vsExtensionPath.FullPath, artifactsPath);
//       CopyFiles(artifactsPath.CombineWithFilePath("TickTrader.Algo.NewsIndicator.ttalgo").FullPath, repoPath);

//       var configuratorInstallPath = serverBinPath.Combine("Configurator");
//       CopyDirectory(configuratorBinPath, configuratorInstallPath);

//       CopyDirectory(pkgLoaderBinPath.FullPath, indicatorHostBinPath);
//       var indicatorHostRuntimePath = indicatorHostBinPath.Combine("bin").Combine("runtime");
//       CopyDirectory(runtimeHostBinPath, indicatorHostRuntimePath);
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("ZipArtifacts")
//    .IsDependentOn("PrepareArtifacts")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("ZipArtifacts") : null;

//    try
//    {
//       Zip(terminalBinPath, artifactsPath.CombineWithFilePath($"AlgoTerminal {buildId}.x64.zip"));
//       Zip(serverBinPath, artifactsPath.CombineWithFilePath($"AlgoServer {buildId}.x64.zip"));
//       Zip(configuratorBinPath, artifactsPath.CombineWithFilePath($"AlgoServer Configurator {buildId}.x64.zip"));
//       Zip(publicApiBinPath, artifactsPath.CombineWithFilePath($"PublicAPI {buildId}.zip"));
//       Zip(symbolStorageBinPath, artifactsPath.CombineWithFilePath($"SymbolStorage {buildId}.x64.zip"));
//       Zip(backtesterApiBinPath, artifactsPath.CombineWithFilePath($"BacktesterApi {buildId}.zip"));
//       Zip(backtesterHostBinPath, artifactsPath.CombineWithFilePath($"BacktesterV1Host {buildId}.x64.zip"));
//       Zip(indicatorHostBinPath, artifactsPath.CombineWithFilePath($"IndicatorHost {buildId}.zip"));
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

Task("ZipGithubArtifacts")
   .IsDependentOn("PublishGithubProjects")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("ZipArtifacts") : null;

   try
   {
      Zip(terminalBinPath, artifactsPath.CombineWithFilePath($"AlgoTerminal {buildId}.x64.zip"));
      Zip(serverBinPath, artifactsPath.CombineWithFilePath($"AlgoServer {buildId}.x64.zip"));
   }
   finally
   {
      block?.Dispose();
   }
});

// Task("CreateInstaller")
//    .IsDependentOn("PrepareArtifacts")
//    .Does(() =>
// {
//    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("CreateInstaller") : null;

//    try
//    {
//       if (!System.IO.File.Exists(nsisPath.MakeAbsolute(Context.Environment).ToString()))
//       {
//          if (BuildSystem.IsLocalBuild)
//             Error("Failed to create installer: NSIS not found!");
//          else
//             throw new Exception("Failed to create installer: NSIS not found!");
//       }

//       CreateUninstallScript(terminalBinPath, setupDirPath.CombineWithFilePath("Terminal.Uninstall.nsi"));
//       CreateUninstallScript(serverBinPath, setupDirPath.CombineWithFilePath("AlgoServer.Uninstall.nsi"));
//       CreateUninstallScript(configuratorBinPath, setupDirPath.CombineWithFilePath("Configurator.Uninstall.nsi"));

//       StartProcess(nsisPath, new ProcessSettings {
//          Arguments = $"/DPRODUCT_BUILD={buildId} {setupDirPath.CombineWithFilePath("Algo.Setup.nsi").FullPath}",
//       });
//    }
//    finally
//    {
//       block?.Dispose();
//    }
// });

// Task("CreateAllArtifacts")
//    .IsDependentOn("ZipArtifacts")
//    .IsDependentOn("CreateInstaller");

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