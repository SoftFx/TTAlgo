///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Push");
var configuration = Argument("configuration", "Release");
var artifactsDir = Argument("artifactsDir", "./artifacts.nuget");
var targetProjects = Argument("targetProjects", "api_tools_templates");
var projects = new Dictionary<string,string> { 
    { "api", "./src/csharp/api/TickTrader.Algo.Api/TickTrader.Algo.Api.csproj" },
    { "tools", "./src/csharp/sdk/TickTrader.Algo.Tools/TickTrader.Algo.Tools.csproj" },
    { "templates", "./src/csharp/sdk/TickTrader.Algo.Templates/TickTrader.Algo.Templates.csproj" },
};
var versionSuffix = Argument("versionSuffix", "");
var nugetKey = Argument("nugetKey", "nevet push real key to git");
var details = Argument<DotNetVerbosity>("details", DotNetVerbosity.Detailed);

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    foreach(var projectName in targetProjects.Split("_"))
    {
        DotNetClean(projects[projectName], new DotNetCleanSettings {
            Configuration = configuration,
            Verbosity = details,
        });
    }
    CleanDirectory(artifactsDir);
});

Task("Pack")
    .IsDependentOn("Clean")
    .Does(() =>
{
    foreach(var projectName in targetProjects.Split("_"))
    {
        DotNetPack(projects[projectName], new DotNetPackSettings {
        Configuration = configuration,
        Verbosity = details,
        VersionSuffix = versionSuffix,
        OutputDirectory = artifactsDir,
    });
    }
});

Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
{
    foreach(var pkg in GetFiles($"{artifactsDir}/*.nupkg"))
    {
        DotNetNuGetPush(pkg, new DotNetNuGetPushSettings {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = nugetKey,
        });
    }
});

PrintArguments();
RunTarget(target);

public void PrintArguments()
{
    Information("target: {0}", target);
    Information("configuration: {0}", configuration);
    Information("artifactsDir: {0}", artifactsDir);
    Information("targetProjects: {0}", targetProjects);
    Information("versionSuffix: {0}", versionSuffix);
    Information("details: {0}", details);
    Information("nuget key invalid: {0}", nugetKey.Contains(' '));
}