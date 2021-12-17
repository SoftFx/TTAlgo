///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = ConsoleOrBuildSystemArgument("Target", "Push");
var buildNumber = ConsoleOrBuildSystemArgument("BuildNumber", 0);
var configuration = ConsoleOrBuildSystemArgument("Configuration", "Release");
var solutionDir = ConsoleOrBuildSystemArgument("SolutionDir", "./");
var artifactsDirName = ConsoleOrBuildSystemArgument("ArtifactsDirName", "artifacts.nuget");
var targetProject = ConsoleOrBuildSystemArgument("TargetProject", "api");
var versionSuffix = ConsoleOrBuildSystemArgument("VersionSuffix", "");
var nugetKey = ConsoleOrBuildSystemArgument("NugetApiKey", "never push real key to git");
var details = ConsoleOrBuildSystemArgument<DotNetVerbosity>("Details", DotNetVerbosity.Detailed);

var solutionPath = DirectoryPath.FromString(solutionDir);
var projects = new Dictionary<string, string> { 
    { "api", solutionPath.Combine("src/csharp/api/TickTrader.Algo.Api/TickTrader.Algo.Api.csproj").ToString() },
    { "tools", solutionPath.Combine("src/csharp/sdk/TickTrader.Algo.Tools/TickTrader.Algo.Tools.csproj").ToString() },
    { "templates", solutionPath.Combine("src/csharp/sdk/TickTrader.Algo.Templates/TickTrader.Algo.Templates.csproj").ToString() },
};
var projectPath = projects[targetProject];
var artifactsPath = solutionPath.Combine(artifactsDirName);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    var exitCode = StartProcess("dotnet", new ProcessSettings {
        WorkingDirectory = solutionDir,
        Arguments = "--info"
    });

    if (exitCode != 0)
        throw new Exception($"Failed to get .NET SDK info: {exitCode}");

    // this is expected to throw if VersionPrefix is not in correct format
    var versionPrefix = new Version(XmlPeek(projectPath, "//VersionPrefix"));

    var pkgVersion = string.IsNullOrEmpty(versionSuffix) ? $"{versionPrefix}" : $"{versionPrefix}-{versionSuffix}.{buildNumber}";
    Information("Calculated package version: {0}", pkgVersion);

    if (BuildSystem.IsRunningOnTeamCity)
        TeamCity.SetBuildNumber(pkgVersion);
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
        DotNetClean(projectPath.ToString(), new DotNetCleanSettings {
            Configuration = configuration,
            Verbosity = details,
        });
        CleanDirectory(artifactsPath);
    }
    finally 
    {
        block?.Dispose();
    }
});

Task("Pack")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("Pack") : null;

    try
    {
        DotNetPack(projectPath.ToString(), new DotNetPackSettings {
            Configuration = configuration,
            Verbosity = details,
            VersionSuffix = versionSuffix,
            OutputDirectory = artifactsPath,
        });
    }
    finally
    {
        block?.Dispose();
    }
});

Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
{
    var block = BuildSystem.IsRunningOnTeamCity ? TeamCity.Block("Push") : null;

    try
    {
        foreach(var pkg in GetFiles($"{artifactsPath}/*.nupkg"))
        {
            DotNetNuGetPush(pkg, new DotNetNuGetPushSettings {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = nugetKey,
            });
        }
    }
    finally
    {
        block?.Dispose();
    }
});

PrintArguments();
RunTarget(target);

public void PrintArguments()
{
    Information("Target: {0}", target);
    Information("BuildNumber: {0}", buildNumber);
    Information("Configuration: {0}", configuration);
    Information("SolutionDir: {0}", solutionDir);
    Information("ArtifactsDirName: {0}", artifactsDirName);
    Information("TargetProject: {0}", targetProject);
    Information("VersionSuffix: {0}", versionSuffix);
    Information("Details: {0}", details);
    Information("Nuget key invalid: {0}", nugetKey.Contains(' '));
}

public string ConsoleOrBuildSystemArgument(string name, string defautValue) => ConsoleOrBuildSystemArgument<string>(name, defautValue);

public T ConsoleOrBuildSystemArgument<T>(string name, T defautValue)
{
    if (HasArgument(name))
        return Argument<T>(name);

    if (BuildSystem.IsRunningOnTeamCity
        && TeamCity.Environment.Build.BuildProperties.TryGetValue(name, out var teamCityProperty))
    {
        Information("Found Teamcity property: {0}={1}", name, teamCityProperty);

        const string envVarName = "env_TempTeamCityProperty";
        Environment.SetEnvironmentVariable(envVarName, teamCityProperty, EnvironmentVariableTarget.Process);
        return EnvironmentVariable<T>(envVarName, defautValue);
    }

    return defautValue;
}
