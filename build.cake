var solution = "DotConsole.sln";

Task("Build").Does(() => {
    DotNetBuild(solution, settings => {

    });
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
            Arguments = "DotConsole.Cmd/bin/Debug/DotConsole.Cmd.exe"
        });
    });

var target = Argument("target", "default");
RunTarget(target);
