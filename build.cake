var solution = "DotConsole.sln";

Task("Build").Does(() => {
    DotNetBuild(solution, settings => {

    });
});

Task("Restore").Does(() => {
    var solutions = GetFiles("./**/*.sln");
    foreach(var sol in solutions)
    {
        Information("Restoring {0}", sol);
        NuGetRestore(sol);
    }
});

Task("Publish-Npm").Does(() => {
    StartProcess("npm", new ProcessSettings {
        Arguments = "publish"
    });
});

Task("Run")
    .IsDependentOn("Build")
    .Does(() => {
        StartProcess("mono", new ProcessSettings {
            Arguments = "DotConsole/bin/Debug/DotConsole.exe"
        });
    });

var target = Argument("target", "default");
RunTarget(target);