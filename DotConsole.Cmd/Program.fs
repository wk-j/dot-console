open DotConsole.Library
open System

[<EntryPoint>]
let main argv = 
      while true do
            let verb = getVerb()
            let command  = verb |> verbCmd
            Console.WriteLine(command)
      0

