
var solution = "DotConsole.sln";

Task("Build").Does(() => {
    DotNetBuild(solution, settings => {

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