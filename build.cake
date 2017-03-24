var solution = "DotConsole.sln";

Task("Build").Does(() => {
    DotNetBuild(solution, settings => {

    });
});

Task("Restore").Does(() => {
    NugetRestore(solution);
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