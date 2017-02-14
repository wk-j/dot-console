open DotConsole.Library
open DotConsole.Executor
open System

let writeLine (msg:string) = Console.WriteLine(" " + msg)

[<EntryPoint>]
let main argv = 
      while true do
            let command = getCommand()
            match command with
            | Valid cmd ->
                  sprintf "Executing command ➯ %s"  cmd |> writeLine
                  let args = cmd.Replace("dotnet", "")
                  executeCommand "dotnet" args 
            | Cancel
            | Quit -> Environment.Exit(0)
      0

