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
      | Console (lang,  out)   -> gen "console" lang out
      | ClassLib (lang,  out)  -> gen "classlib" lang out
      | MsTest (lang,  out)    -> gen "mstest" lang out
      | XUnit (lang, out)     -> gen "xunit" lang  out
      | X                           -> ""

let verbCmd verb =
      match verb with
      | New project -> sprintf "dotnet new %s" (projectCmd project)
      | Add (path, reference) -> ""
      | List (path, listType) -> ""


let color = Konsole.ForegroundColor

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

let readLine (info:string) options (defaultValue: Option<string>)   = 
      color |> setColor
      ConsoleColor.Green |> setColor
      Konsole.WriteLine("" + info)
      color |> setColor
      let mutable index = 1
      for k, v in options do
            let msg = sprintf "‣ %-10s %-20s" k v
            Konsole.WriteLine(msg)
            index <- index + 1
      Konsole.Write("⤷ ")
      ConsoleColor.Yellow |> setColor
      (*
      match defaultValue with
      | Some v -> SendKeys.SendWait(v)
      | None -> ()
      *)
      Konsole.ReadLine()

let rec getOutput() =
      let options = []
      let value = readLine "Output directory" options None
      match value with 
      | "" -> getOutput() 
      | x -> OutputDirectory(x)

let rec getLang() =
      let options = [
            ("C#", "C#")
            ("F#", "F#")
      ]
      let value = readLine "Language" options (Some "C#") 
      match value with
      | "F#"  -> FSharp 
      | "C#" -> CSharp
      | x -> getLang()

let rec getType() =
      let options = [
            ("console", "Console Application")
            ("classlib", "Class library")
            ("mstest", "Unit Test Project")
            ("xunit", "xUnit Test Project")
            ("mvc", "MVC ASP.NET Core Web Application")
            ("webapi", "Web API ASP.NET Core Web Application")
            ("web", "Empty ASP.NET Core Web Application")
            ("sln", "Solution File")
      ]
      let value = readLine "Projec Type" options (Some "console")
      match value with
      | "console" -> Console(getLang(), getOutput())
      | "web" -> Web(getOutput())
      | x -> getType() 

let getVerb str =
      let options = [
            ("new", "Initialize .NET projects")
            ("restore", "Restore dependencies specified in the .NET project")
            ("build", "Builds a .NET project")
            ("publish", "Publishes a .NET project for deployment (including the runtime")
            ("run", "Compiles and immediately executes a .NET project")
            ("test", "Runs unit tests using the test runner specified in the project")
            ("pack", "Creates a NuGet package")
            ("migrate", "Migrates a project.json based project to a msbuild based project")
            ("clean", "Clean build output(s)")
            ("sln", "Modify solution (SLN) files")
            ("add", "Add items to the project")
            ("remove", "Remove items from the project")
            ("list", "List items in the project")
      ]
      let value = readLine "Command" options (Some "new")
      match value with
      | "new" -> New(getType())
      | x -> New(getType())

