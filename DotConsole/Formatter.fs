module DotConsole.Formatter

open System

type Konsole = System.Console

type Text =
      | Info of string
      | Prompt of string
      | Key of string
      | Description of string

let write text =
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
      Info(info) |> write
      for k, v in options do
            let key = sprintf " %-15s" k
            let desc = sprintf "%-20s" v
            Key(key) |> write
            Description(desc) |> write
      Prompt("➟ ") |> write
      Console.ReadLine()