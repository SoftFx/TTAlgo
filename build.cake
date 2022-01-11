#tool nuget:?package=vswhere&version=2.8.4

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = ConsoleOrBuildSystemArgument("Target", "CreateAllArtifacts");
var buildNumber = ConsoleOrBuildSystemArgument("BuildNumber", 0);
var version = ConsoleOrBuildSystemArgument("Version", "1.19");
var configuration = ConsoleOrBuildSystemArgument("Configuration", "Release");
var sourcesDir = ConsoleOrBuildSystemArgument("SourcesDir", "./");
var artifactsDirName = ConsoleOrBuildSystemArgument("ArtifactsDirName", "artifacts.build");
var details = ConsoleOrBuildSystemArgument<DotNetVerbosity>("Details", DotNetVerbosity.Normal);
var skipTests = ConsoleOrBuildSystemArgument("SkipTests", false); // used on TeamCity to enable test results integration
var nsisDirPath = ConsoleOrBuildSystemArgument("NsisPath", @"c:/Program Files (x86)/NSIS/");
var msBuildDirPath = ConsoleOrBuildSystemArgument("MSBuildPath", "");

var solutionDirPath = DirectoryPath.FromString(sourcesDir);
var publishPath = solutionDirPath.Combine("bin");
var buildId = $"{version}.{buildNumber}.0";
var artifactsPath = solutionDirPath.Combine(artifactsDirName);
var mainSolutionPath = solutionDirPath.CombineWithFilePath("Algo.sln");
var sdkSolutionPath = solutionDirPath.CombineWithFilePath("src/csharp/TickTrader.Algo.Sdk.sln");
var nsisPath = DirectoryPath.FromString(nsisDirPath).CombineWithFilePath("makensis.exe");
var publishProjectPaths = new Dictionary<string, FilePath>()
{
   { "terminal", solutionDirPath.CombineWithFilePath("TickTrader.BotTerminal/TickTrader.BotTerminal.csproj") },
   { "configurator", solutionDirPath.CombineWithFilePath("TickTrader.BotAgent.Configurator/TickTrader.BotAgent.Configurator.csproj") },
   { "server", solutionDirPath.CombineWithFilePath("TickTrader.BotAgent/TickTrader.BotAgent.csproj") },
   { "public-api", solutionDirPath.CombineWithFilePath("src/csharp/core/TickTrader.Algo.Server.PublicAPI/TickTrader.Algo.Server.PublicAPI.csproj") },
};
var vsExtensionPath = solutionDirPath.CombineWithFilePath($"src/csharp/sdk/TickTrader.Algo.VS.Package/bin/${configuration}/TickTrader.Algo.VS.Package.vsix");

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

// Teardown(ctx =>
// {
// });

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
      CleanDirectory(publishPath);
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
         Information("Looking for MSBuild with VS extension SDK. File '{0}' doesn't exists");

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
      var testProjects = GetFiles(solutionDirPath.Combine("src/csharp/tests/**/*.csproj").ToString());
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
      var projectPath = publishProjectPaths["terminal"];

      // we need to change post-build tasks to work with publish
      DotNetBuild(projectPath.ToString(), new DotNetBuildSettings {
         Configuration = configuration,
         Verbosity = details,
         NoRestore = true,
         OutputDirectory = publishPath.Combine("terminal"),
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
      var projectPath = publishProjectPaths["configurator"];

      DotNetPublish(projectPath.ToString(), new DotNetPublishSettings {
         Configuration = configuration,
         Verbosity = details,
         NoBuild = true,
         OutputDirectory = publishPath.Combine("configurator"),
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
      var projectPath = publishProjectPaths["server"];

      DotNetPublish(projectPath.ToString(), new DotNetPublishSettings {
         Configuration = configuration,
         Verbosity = details,
         OutputDirectory = publishPath.Combine("server"),
      });
   }
   finally
   {
      block?.Dispose();
   }
});

Task("PublishPublicApi")
   .IsDependentOn("Test")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PublishPublicApi") : null;
   
   try
   {
      var projectPath = publishProjectPaths["public-api"];

      DotNetPublish(projectPath.ToString(), new DotNetPublishSettings {
         Configuration = configuration,
         Verbosity = details,
         NoBuild = true,
         OutputDirectory = publishPath.Combine("public-api"),
         Framework = "net472"
      });

      DeleteFiles(publishPath.CombineWithFilePath("public-api/libgrpc_csharp_ext*").ToString());
   }
   finally
   {
      block?.Dispose();
   }
});

Task("PrepareArtifacts")
   .IsDependentOn("PublishTerminal")
   .IsDependentOn("PublishConfigurator")
   .IsDependentOn("PublishServer")
   .IsDependentOn("PublishPublicApi")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("PrepareArtifacts") : null;

   try
   {
      CreateDirectory(publishPath.Combine("terminal/Redist/"));
      CreateDirectory(publishPath.Combine("terminal/AlgoRepository/"));
      CopyFiles(vsExtensionPath.ToString(), publishPath.Combine("terminal/Redist/"));
      CopyFiles(artifactsPath.CombineWithFilePath("TickTrader.Algo.NewsIndicator.ttalgo").ToString(), publishPath.Combine("terminal/AlgoRepository/"));

      CreateDirectory(publishPath.Combine("server/Configurator/"));
      CopyFiles(publishPath.Combine("configurator/**/*.*").ToString(), publishPath.Combine("server/Configurator/"));
   }
   finally
   {
      block?.Dispose();
   }
});

Task("ZipArtifacts")
   .IsDependentOn("PrepareArtifacts")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("ZipArtifacts") : null;

   try
   {
      Zip(publishPath.Combine("terminal"), artifactsPath.CombineWithFilePath($"AlgoTerminal {buildId}.x64.zip"));
      Zip(publishPath.Combine("server"), artifactsPath.CombineWithFilePath($"AlgoServer {buildId}.x64.zip"));
      Zip(publishPath.Combine("configurator"), artifactsPath.CombineWithFilePath($"AlgoServer Configurator {buildId}.x64.zip"));
      Zip(publishPath.Combine("public-api"), artifactsPath.CombineWithFilePath($"PublicAPI {buildId}.net472.zip"));
   }
   finally
   {
      block?.Dispose();
   }
});

Task("CreateInstaller")
   .IsDependentOn("PrepareArtifacts")
   .Does(() =>
{
   var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("CreateInstaller") : null;
   
   try
   {
      if (!System.IO.File.Exists(nsisPath.MakeAbsolute(Context.Environment).ToString()))
      {
         if (BuildSystem.IsLocalBuild)
            Error("Failed to create installer: NSIS not found!");
         else
            throw new Exception("Failed to create installer: NSIS not found!");
      }
   }
   finally
   {
      block?.Dispose();
   }
});

Task("CreateAllArtifacts")
   .IsDependentOn("ZipArtifacts")
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