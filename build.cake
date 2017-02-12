
var solution = "DotConsole.sln";

Task("Build").Does(() => {
    DotNetBuild(solution, settings => {

    });
});

var target = Argument("target", "default");
RunTarget(target);