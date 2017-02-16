module DotConsole.Library

open System
open System.Windows.Forms
open System.Threading.Tasks
open DotConsole.Formatter
open System.IO
open System.Linq

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
    static member ($) (GetValue, (PackageName v)) = v

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
      | ProjectReference of ProjectPath
      | PackageReference of PackageName

type ListType =
      | Reference
      | Package

type Verb =
      | New of Project
      | Add of ProjectPath * Reference
      | Remove of ProjectPath * Reference
      | List of ProjectPath * ListType
      | Restore of ProjectPath
      | Build of ProjectPath
      | Run of ProjectPath
      | Clean of ProjectPath

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

let getReference = function
      | PackageReference package -> sprintf "package %s" (value package)
      | ProjectReference project -> sprintf "reference %s" (value project)

let verbCmd verb =
      let f = sprintf
      match verb with
      | New project -> f "dotnet new %s" (projectCmd project)
      | Add (path, reference) -> f "dotnet add %s %s" (value path) (getReference reference)
      | Remove (path, reference) -> f "dotnet remove %s %s" (value path) (getReference reference)
      | List (path, listType) -> ""
      | Build path -> f "dotnet build %s" (value path)
      | Restore path -> f "dotnet restore %s" (value path)
      | Run path -> f "dotnet run --project %s" (value path)
      | Clean path -> f "dotnet clean %s" (value path)

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

let rec getProject() =
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
      | x -> getProject()

let getProjects() =
      let dir = System.IO.DirectoryInfo("./")
      let current = dir.FullName

      let toRelative (path:string) =
            path.Replace(current, "")

      let getFile(pattern) =
            dir.GetFiles(pattern, System.IO.SearchOption.AllDirectories)
                        .Select(fun x -> x.FullName).ToArray() |> Array.map toRelative

      let files = 
            [| getFile("*.csproj")
               getFile("*.fsproj")
               getFile("*.sln") |]  |> Array.collect id 
      (files)

let selectProject(title: string, files) =
      let options = files |> Array.map (fun x -> ("", x))
      let value = readInput title options None
      let ok, number = Int32.TryParse(value)
      if ok then 
            let index = number - 1
            if options.Length >= index then
                  let proj = options.[index] |> snd
                  Some <| ProjectPath(proj)
            else None
      else 
            None

let rec buildCommand() =
      let files = getProjects()
      let project = selectProject("Select project / solution to restore packages", files)
      match project with 
      | Some x -> Build(x)
      | None -> buildCommand()

let rec restoreCommand() =
      let files = getProjects()
      let project = selectProject("Select project / solution to restore packages", files)
      match project with 
      | Some x -> Restore(x)
      | None -> restoreCommand()

let rec runCommand() =
      let files = getProjects()
      let project = selectProject("Select project to run", files)
      match project with 
      | Some x -> Run(x)
      | None -> runCommand()

let rec cleanCommand() =
      let files = getProjects()
      let project = selectProject("Select project to clean", files)
      match project with
      | Some x -> Clean(x)
      | None -> cleanCommand()

let rec projectPath() =
      let files = getProjects()
      let project = selectProject("Select project to add reference/package", files)
      match project with
      | Some x -> x
      | None -> projectPath() 

let rec referenceCommand() =
      let options = [
            ("r reference", "Add reference project")
            ("p package", "Add nuget package")
      ]

      let reference = readInput "Select reference type" options None

      match reference with
      | "r" -> ProjectPath(reference) |> ProjectReference
      | "p" -> 
            let package = readInput "Enter package name" [] None
            PackageName(package) |> PackageReference
      | x -> referenceCommand()

let rec addCommand() =
      let files = getProjects()
      let project = selectProject("Select project to add reference/package", files)
      match project with
      | Some x -> Add(x, referenceCommand())
      | None -> addCommand() 

let rec removeCommand() =
      let files = getProjects()
      let project = selectProject("Select project to remove reference/package", files)
      match project with
      | Some x -> Remove(x, referenceCommand())
      | None -> removeCommand() 

let getCommand str =
      let options = [
            ("n new", "Initialize .NET projects")
            ("r restore", "Restore dependencies specified in the .NET project")
            ("b build", "Builds a .NET project")
            ("p publish", "Publishes a .NET project for deployment (including the runtime")
            ("u run", "Compiles and immediately executes a .NET project")
            ("t test", "Runs unit tests using the test runner specified in the project")
            ("k pack", "Creates a NuGet package")
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
            | "n" | "new" -> New(getProject()) 
            | "r" | "restore" -> restoreCommand()
            | "b" | "build" -> buildCommand() 
            | "u" | "run" -> runCommand()
            | "a" | "add" -> addCommand()
            | "c" | "clean" -> cleanCommand()
            | "v" | "remove " -> removeCommand()
            | x -> restoreCommand() 

      command |> verbCmd |> Valid