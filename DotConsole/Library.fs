module DotConsole.Library

open System
open System.Windows.Forms
open System.Threading.Tasks
open DotConsole.Formatter

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

type InputStatus =
      | Valid of string
      | Cancel
      | Quit

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
      let f = sprintf
      let gen types lang out =
            let path = value out
            let language = langCmd lang
            f "%s --language %s --output %s" types language path

      match project with
      | Console (lang,  out) -> gen "console" lang out
      | ClassLib (lang,  out) -> gen "classlib" lang out
      | MsTest (lang,  out) -> gen "mstest" lang out
      | XUnit (lang, out) -> gen "xunit" lang  out
      | Mvc (lang, out) -> gen "mvc" lang out
      | Sln (sln, out) -> f "--name %s --output %s" (value sln) (value out)
      | Web (out) -> f "--output %s" (value out)
      | WebApi (out) -> f "--output %s" (value out)

let verbCmd verb =
      let f = sprintf
      match verb with
      | New project -> f "dotnet new %s" (projectCmd project)
      | Add (path, reference) -> ""
      | List (path, listType) -> ""

let rec getOutput() =
      let options = []
      let value = readInput "Output directory" options None
      match value with 
      | "" -> getOutput() 
      | x -> OutputDirectory(x)

let rec getLang() =
      let options = [
            ("c C#", "C# langauge")
            ("f F#", "F# language")
      ]
      let value = readInput "Language" options (Some "C#") 
      match value with
      | "f"  -> FSharp 
      | "c" -> CSharp
      | x -> getLang()

let rec getSolution() =
      let value = readInput "SolutionName" [] None
      value |> SolutionName

let rec getType() =
      let options = [
            ("c console", "Console Application")
            ("l classlib", "Class library")
            ("t mstest", "Unit Test Project")
            ("x xunit", "xUnit Test Project")
            ("m mvc", "MVC ASP.NET Core Web Application")
            ("a webapi", "Web API ASP.NET Core Web Application")
            ("w web", "Empty ASP.NET Core Web Application")
            ("s sln", "Solution File")
      ]
      let value = readInput "Projec Type" options (Some "console")

      let langAndOut() =
            (getLang(), getOutput())

      let nameAndOutput() =
            (getSolution(), getOutput())

      match value with
      | "c" | "console" -> langAndOut() |> Console
      | "t" | "mstest" -> langAndOut() |> MsTest 
      | "l" | "classlib" -> langAndOut() |> ClassLib
      | "x" | "xunit" -> langAndOut() |> XUnit
      | "m" | "mvc" -> langAndOut() |> Mvc
      | "a" | "webapi" -> getOutput() |> WebApi
      | "w" | "web" -> getOutput() |> Web
      | "s" | "sln" -> nameAndOutput() |> Sln
      | x -> getType() 

let getCommand str =
      let options = [
            ("n new", "Initialize .NET projects")
            ("r restore", "Restore dependencies specified in the .NET project")
            ("b build", "Builds a .NET project")
            ("p publish", "Publishes a .NET project for deployment (including the runtime")
            ("u run", "Compiles and immediately executes a .NET project")
            ("t test", "Runs unit tests using the test runner specified in the project")
            ("c pack", "Creates a NuGet package")
            ("m migrate", "Migrates a project.json based project to a msbuild based project")
            ("c clean", "Clean build output(s)")
            ("s sln", "Modify solution (SLN) files")
            ("a add", "Add items to the project")
            ("v remove", "Remove items from the project")
            ("l list", "List items in the project")
      ]

      let confirm msg =
            let confirm = readInput "Press Enter to confirm (cc: Cancel) (qq:Quit)" [] None
            if confirm.EndsWith("cc") then Cancel
            elif confirm.EndsWith("qq") then Quit
            else Valid confirm

      let value = readInput "Command" options (Some "new")
      let command = 
            match value with
            | "n" | "new" -> New(getType()) 
            | "r" | "restore" -> New(getType()) 
            | "b" | "build" -> New(getType()) 
            | x -> New(getType()) 

      command |> verbCmd |> Valid