module DotConsole.Library

open System
open System.Windows.Forms
open System.Threading.Tasks

type Konsole = System.Console

type SolutionName = SolutionName of string
type ProjectName = ProjectName of string
type OutputDirectory = OutputDirectory of string
type ProjectPath = ProjectPath of string
type SolutionPath = SolutionPath of string
type PackageName = PackageName of string

type GetValue = GetValue with
    static member ($) (GetValue, (SolutionName v)) = v
    static member ($) (GetValue, (ProjectName v)) = v
    static member ($) (GetValue, (ProjectPath v)) = v
    static member ($) (GetValue, (OutputDirectory v)) = v

let inline value x : string = GetValue $ x

type Language =
      | FSharp
      | CSharp

type Project =
      | Console of Language * OutputDirectory
      | ClassLib of Language * OutputDirectory
      | MsTest of Language *  OutputDirectory
      | XUnit of Language * OutputDirectory
      | Web of  OutputDirectory
      | Mvc of Language * OutputDirectory
      | WebApi of OutputDirectory
      | Sln of SolutionName * OutputDirectory

type Reference =
      | Project of ProjectPath
      | Package of PackageName

type ListType =
      | Reference
      | Package

type Verb =
      | New of Project
      | Add of ProjectPath * Reference
      | List of ProjectPath * ListType

let langCmd lang =
      match lang with
      | FSharp -> "F#"
      | CSharp -> "C#"

let projectCmd project =

      let gen types lang out =
            let path = value out
            "{type} --language {lange} --output {out}"
                  .Replace("{type}", types)
                  .Replace("{lange}", langCmd lang)
                  .Replace("{out}", path)

      match project with
      | Console (lang,  out) -> gen "console" lang out
      | ClassLib (lang,  out) -> gen "classlib" lang out
      | MsTest (lang,  out) -> gen "mstest" lang out
      | XUnit (lang, out) -> gen "xunit" lang  out
      | X -> ""

let verbCmd verb =
      match verb with
      | New project -> sprintf "dotnet new %s" (projectCmd project)
      | Add (path, reference) -> ""
      | List (path, listType) -> ""


// Color
let foreColor = ConsoleColor.White 
let answerColor = ConsoleColor.Yellow
let titleColor = ConsoleColor.Green
let promptColor = ConsoleColor.Red

// IO
let write (x:string) = Konsole.Write(x)
let writeLine(x: string) = Konsole.WriteLine(x)
let readLine = Konsole.ReadLine

let setColor color =
      Konsole.ForegroundColor <- color

let init() =
        let mutable key = new ConsoleKeyInfo();

        while true do
            key <- Console.ReadKey(true)

            match key.Key with
            | ConsoleKey.UpArrow -> SendKeys.Send("XX")
            | ConsoleKey.DownArrow -> SendKeys.Send("YY")
            | ConsoleKey.Escape  -> Environment.Exit(0)
            | x -> ()

let readInput (info:string) options (defaultValue: Option<string>)   = 

      setColor titleColor
      writeLine info
      setColor foreColor
      let mutable index = 1
      for k, v in options do
            let key = sprintf "‣ %-15s" k
            let desc = sprintf "%-20s" v
            setColor titleColor
            write key
            setColor foreColor
            writeLine desc
            index <- index + 1
      setColor promptColor
      write("⤷ ")
      setColor answerColor
      (*
      match defaultValue with
      | Some v -> SendKeys.SendWait(v)
      | None -> ()
      *)
      readLine()

let rec getOutput() =
      let options = []
      let value = readInput "Output directory" options None
      match value with 
      | "" -> getOutput() 
      | x -> OutputDirectory(x)

let rec getLang() =
      let options = [
            ("[c] C#", "C# langauge")
            ("[f] F#", "F# language")
      ]
      let value = readInput "Language" options (Some "C#") 
      match value with
      | "f"  -> FSharp 
      | "c" -> CSharp
      | x -> getLang()

let rec getType() =
      let options = [
            ("[c] console", "Console Application")
            ("[l] classlib", "Class library")
            ("[t] mstest", "Unit Test Project")
            ("[x] xunit", "xUnit Test Project")
            ("[m] mvc", "MVC ASP.NET Core Web Application")
            ("[a] webapi", "Web API ASP.NET Core Web Application")
            ("[w] web", "Empty ASP.NET Core Web Application")
            ("[s] sln", "Solution File")
      ]
      let value = readInput "Projec Type" options (Some "console")
      match value with
      | "c" | "console" -> Console(getLang(), getOutput())
      | "w" | "web" -> Web(getOutput())
      | x -> getType() 

let getCommand str =
      let options = [
            ("[n] new", "Initialize .NET projects")
            ("[r] restore", "Restore dependencies specified in the .NET project")
            ("[b] build", "Builds a .NET project")
            ("[p] publish", "Publishes a .NET project for deployment (including the runtime")
            ("[u] run", "Compiles and immediately executes a .NET project")
            ("[t] test", "Runs unit tests using the test runner specified in the project")
            ("[c] pack", "Creates a NuGet package")
            ("[m] migrate", "Migrates a project.json based project to a msbuild based project")
            ("[c] clean", "Clean build output(s)")
            ("[s] sln", "Modify solution (SLN) files")
            ("[a] add", "Add items to the project")
            ("[v] remove", "Remove items from the project")
            ("[l] list", "List items in the project")
      ]
      let value = readInput "Command" options (Some "new")
      match value with
      | "n" | "new" -> New(getType())
      | "r" | "restore" -> New(getType())
      | "b" | "build" -> New(getType())
      | x -> New(getType())

