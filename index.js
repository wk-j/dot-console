#!/usr/bin/env node

var spawn = require('child_process').spawn;
var process = require("process");

var file = __dirname + "/DotConsole.Cmd/bin/Debug/DotConsole.Cmd.exe";

if(process.platform === "win32") {
    spawn(file, [], { stdio: "inherit"});
} else {
    spawn("mono", [file] , {stdio: "inherit"});
}
