module DotConsole.Executor

open System
open System.Diagnostics

type private Message =
    | Info of string
    | Error of string

let private write message = 

    let write (msg:string) color =
        if not <| String.IsNullOrEmpty(msg) then
            Console.ForegroundColor <- color 
            Console.WriteLine(" {0}", msg)

    match message with
    | Info str -> write str ConsoleColor.White
    | Error str ->  write str ConsoleColor.Red

let executeCommand cmd args=
    Console.ForegroundColor <- ConsoleColor.White
    let info = ProcessStartInfo()
    info.FileName <- cmd
    info.Arguments <- args
    //info.RedirectStandardOutput <- true
    info.UseShellExecute <- true
    //info.RedirectStandardError <- true

    let outputHandler (s:DataReceivedEventArgs) = 
        s.Data |> Info |> write

    let errorHandler (s: DataReceivedEventArgs) =
        s.Data |> Error |> write

    let ps = new Process()
    ps.StartInfo <- info
    ps.Start() |> ignore
    
    //ps.OutputDataReceived.Add(outputHandler)
    //ps.BeginOutputReadLine()

    //ps.ErrorDataReceived.Add(errorHandler)
    //ps.BeginErrorReadLine()

    ps.WaitForExit() |> ignore
