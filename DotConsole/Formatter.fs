module DotConsole.Formatter

open System

type private Konsole = System.Console

type private Text =
      | Info of string
      | Prompt of string
      | Key of string
      | Description of string

let private write text =
    let setColor color = Konsole.ForegroundColor <- color
    let writeLine x = Konsole.WriteLine(x.ToString())
    let write x = Konsole.Write(x.ToString())
    match text with
    | Info text ->
        setColor ConsoleColor.Green
        writeLine text
    | Prompt text ->
        setColor ConsoleColor.Red
        write text
        setColor ConsoleColor.Yellow
    | Key text ->
        setColor ConsoleColor.Green
        write text
    | Description text ->
        setColor ConsoleColor.White
        writeLine text

let readInput (info:string) options (defaultValue: Option<string>)   = 
      let f = sprintf
      Info(info) |> write
      let mutable index = 1
      for k, v in options do
            let key = 
                if k = "" then f " %-2d" index
                else f " %-15s" k
            let desc = f "%-20s" v
            Key(key) |> write
            Description(desc) |> write
            index <- index + 1
      Prompt("➟ ") |> write
      Console.ReadLine()