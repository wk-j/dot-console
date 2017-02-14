module DotConsole.Executor

open System
open System.Diagnostics

let executeCommand cmd args=
    let info = ProcessStartInfo()
    info.FileName <- cmd
    info.Arguments <- args
    info.RedirectStandardOutput <- true
    info.UseShellExecute <- false
    info.RedirectStandardError <- true

    let outputHandler (s:DataReceivedEventArgs) = 
        Console.WriteLine(s.Data)

    let errorHandler (s: DataReceivedEventArgs) =
        Console.WriteLine(s.Data)

    let ps = new Process()
    ps.StartInfo <- info
    ps.Start() |> ignore
    
    ps.OutputDataReceived.Add(outputHandler)
    ps.BeginOutputReadLine()

    ps.ErrorDataReceived.Add(errorHandler)
    ps.BeginErrorReadLine()

    ps.WaitForExit() |> ignore
