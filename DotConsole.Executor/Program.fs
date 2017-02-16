open System.Diagnostics
open System
open System.IO
open System.Reflection

[<EntryPoint>]
let main argv =   

    let info = ProcessStartInfo()
    info.UseShellExecute <- true

    let path = System.Reflection.Assembly.GetExecutingAssembly().Location;
    let dir = FileInfo(path).Directory.FullName

    let file = Path.Combine(dir, "DotConsole.Cmd.exe")

    let platform = System.Environment.OSVersion.Platform

    match platform with
    | PlatformID.Unix -> 
        info.FileName <- "mono"
        info.Arguments <- file
    | _ -> 
        info.FileName <- file

    use ps = new Process()
    ps.StartInfo <- info
    ps.Start() |> ignore
    ps.WaitForExit()

    0