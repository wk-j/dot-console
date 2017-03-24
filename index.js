#!/usr/bin/env node

var spawn = require('child_process').spawn;
//var process = require("process");

var file = __dirname + "/DotConsole/bin/Debug/DotConsole.exe";

if(process.platform === "win32") {
    spawn(file, [], { stdio: "inherit"});
} else {
    spawn("mono", [file] , {stdio: "inherit"});
}
