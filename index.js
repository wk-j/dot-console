#!/usr/bin/env node

var shell = require("shelljs");
var spawn = require('child_process').spawn;

var file = __dirname + "/DotConsole.Executor/bin/Debug/DotConsole.Executor.exe";
//shell.exec("mono " + file);

spawn("mono", [file] , {stdio: 'inherit'});
