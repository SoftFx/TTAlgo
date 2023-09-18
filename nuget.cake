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

var isGithubBuild = BuildSystem.IsRunningOnGitHubActions;

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

    // simplifies GHA workflow
    if (isGithubBuild && versionSuffix == "none")
        versionSuffix = string.Empty;

    // this is expected to throw if VersionPrefix is not in correct format
    var versionPrefix = new Version(XmlPeek(projectPath, "//VersionPrefix"));

    var pkgVersion = versionPrefix.ToString();
    if (!string.IsNullOrEmpty(versionSuffix))
    {
        versionSuffix = $"{versionSuffix}.{buildNumber}";
        pkgVersion = $"{versionPrefix}-{versionSuffix}";
    }
    Information("Calculated package version: {0}", pkgVersion);

    if (BuildSystem.IsRunningOnTeamCity)
        TeamCity.SetBuildNumber(pkgVersion);
});

// Teardown(ctx =>
// {
// });

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
    DotNetClean(projectPath.ToString(), new DotNetCleanSettings {
        Configuration = configuration,
        Verbosity = details,
    });
    CleanDirectory(artifactsPath);
});

Task("Pack")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetPack(projectPath.ToString(), new DotNetPackSettings {
        Configuration = configuration,
        Verbosity = details,
        VersionSuffix = versionSuffix,
        OutputDirectory = artifactsPath,
    });
});

Task("Push")
    .IsDependentOn("Pack")
    .Does(() => PushToNugetOrg());

Task("PushStandalone")
    .Does(() => PushToNugetOrg());

private void PushToNugetOrg()
{
    foreach(var pkg in GetFiles($"{artifactsPath}/*.nupkg"))
    {
        DotNetNuGetPush(pkg, new DotNetNuGetPushSettings {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = nugetKey,
        });
    }
}

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
    Information("VersionSuffix: '{0}'", versionSuffix);
    Information("Details: {0}", details);
    if (!isGithubBuild)
        Information("NugetKey: {0}", HidePartially(nugetKey));
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

public string HidePartially(string src)
{
    var len = src.Length;
    var sb = new StringBuilder();
    var i = 0;
    while (i < len)
    {
        var cnt = Math.Min(5, len - i);
        if (i % 10 == 0)
            sb.Append(src, i, cnt);
        else 
            sb.Append('*', cnt);

        i += cnt;
    }
    return sb.ToString();
}
