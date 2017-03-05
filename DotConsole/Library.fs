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
type Version = Version of string
type PackageName = PackageName of string 

type GetValue = GetValue with
    static member ($) (GetValue, (SolutionName v)) = v
    static member ($) (GetValue, (ProjectName v)) = v
    static member ($) (GetValue, (ProjectPath v)) = v
    static member ($) (GetValue, (OutputDirectory v)) = v
    static member ($) (GetValue, (Version v)) =v
    static member ($) (GetValue, (PackageName v)) =v

let inline private value x : string = GetValue $ x

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
      | PackageReference of PackageName * Version option

type ListType =
      | Reference
      | Package of Version option

type SolutionVerb =
      | AddProject
      | RemoveProject

type Verb =
      | New of Project
      | Add of ProjectPath * Reference
      | Remove of ProjectPath * Reference
      | List of ProjectPath * ListType
      | Restore of ProjectPath
      | Build of ProjectPath
      | Run of ProjectPath
      | Clean of ProjectPath
      | Test of ProjectPath
      | Solution of ProjectPath * SolutionVerb * ProjectPath
      | Skip
      | Last of Verb

let private langCmd lang =
      match lang with
      | FSharp -> "F#"
      | CSharp -> "C#"

let private projectCmd project =
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
      | Sln (sln, out) -> f "sln --name %s --output %s" (value sln) (value out)
      | Web (out) -> f "web --output %s" (value out)
      | WebApi (out) -> f "webapi --output %s" (value out)

let private getReference = function
      | PackageReference (package, version) -> 
            match version with
            | Some version -> sprintf "package %s --version %s" (value package) (value version) 
            | None -> sprintf "package %s" (value package)
      | ProjectReference project -> sprintf "reference %s" (value project)

let convertToCommandLine verb =
      let f = sprintf
      match verb with
      | New project -> f "dotnet new %s" (projectCmd project) |> Some
      | Add (path, reference) -> f "dotnet add %s %s" (value path) (getReference reference) |> Some
      | Remove (path, reference) -> f "dotnet remove %s %s" (value path) (getReference reference) |> Some
      | List (path, listType) -> "" |> Some
      | Build path -> f "dotnet build %s" (value path) |> Some
      | Restore path -> f "dotnet restore %s" (value path) |> Some
      | Run path -> f "dotnet run --project %s" (value path) |> Some
      | Clean path -> f "dotnet clean %s" (value path) |> Some
      | Test path -> f "dotnet test %s" (value path) |> Some
      | Solution (sln, add, project) ->
            let add = 
                  match add with
                  | AddProject -> "add"
                  | RemoveProject -> "remove"
            f "dotnet sln %s %s %s" (value sln) add (value project) |> Some
      | Skip -> None

let rec private getOutput() =
      let options = []
      let value = readInput "Output directory" options None
      match value with 
      | "" -> getOutput() 
      | x -> OutputDirectory(x)

let rec private getLang() =
      let options = [
            ("c C#", "C# langauge")
            ("f F#", "F# language")
      ]
      let value = readInput "Language" options (Some "C#") 
      match value with
      | "f"  -> FSharp 
      | "c" -> CSharp
      | x -> getLang()

let rec private getSolution() =
      let value = readInput "SolutionName" [] None
      value |> SolutionName

let rec private getProject() =
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

let private getProjects() =
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

let private selectProject (title: string) files =
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

let rec private buildCommand() =
      let files = getProjects()
      let project = selectProject "Select project / solution to restore packages" files
      match project with 
      | Some x -> Build(x)
      | None -> buildCommand()

let rec private restoreCommand() =
      let files = getProjects()
      let project = selectProject "Select project / solution to restore packages" files
      match project with 
      | Some x -> Restore(x)
      | None -> restoreCommand()

let rec private runCommand() =
      let files = getProjects()
      let project = selectProject "Select project to run" files
      match project with 
      | Some x -> Run(x)
      | None -> runCommand()

let rec private cleanCommand() =
      let files = getProjects()
      let project = selectProject"Select project to clean" files
      match project with
      | Some x -> Clean(x)
      | None -> cleanCommand()

let rec private projectPath title =
      let files = getProjects()
      let project = selectProject title  files
      match project with
      | Some x -> x
      | None -> projectPath title 

let rec private testCommand() =
      let project = projectPath "Select project to test"
      Test(project)

let rec addOrRemoveCommand() =
      let options = [
            ("a add", "Add project to solution")
            ("r remove", "Remove project from solution")
      ]

      let add = readInput "Add or remove" options None
      match add with
      | "a" -> AddProject
      | "r" -> RemoveProject
      | _ -> addOrRemoveCommand()

let rec slnCommand() =
      let projects = getProjects()
      let sln = projects |> Array.filter (fun x -> x.EndsWith(".sln")) 
      let solution = selectProject "Select solution" sln
      match solution with
      | Some sln -> 
            let add = addOrRemoveCommand()
            let project = selectProject "Select project to add or remove" projects
            match project with
            | Some proj -> Solution(sln,add,proj)
            | None -> slnCommand()
      | None -> slnCommand()

let rec private referenceCommand() =
      let options = [
            ("r reference", "Add reference project")
            ("p package", "Add nuget package")
      ]

      let reference = readInput "Select reference type" options None

      match reference with
      | "r" -> 
            let project = projectPath("Select project")
            project |> ProjectReference
      | "p" -> 
            let package = readInput "Enter package name" [] None
            let token = System.Text.RegularExpressions.Regex.Split(package, "\\s")
            match token.Length > 1 with
            | true ->
                  let package = token.[0] |> PackageName
                  let version = token.[1]
                  PackageReference(package, Version(version) |> Some) 
            | false -> 
                  let package = PackageName(package)
                  PackageReference(package, None) 
      | x -> referenceCommand()

let rec private addCommand() =
      let files = getProjects()
      let project = selectProject "Select project to add reference/package" files
      match project with
      | Some x -> Add(x, referenceCommand())
      | None -> addCommand() 

let rec private removeCommand() =
      let files = getProjects()
      let project = selectProject "Select project to remove reference/package" files
      match project with
      | Some x -> Remove(x, referenceCommand())
      | None -> removeCommand() 

let mutable last : Verb option = None

let getCommand str =
      let options = [
            ("n new", "Initialize .NET projects")
            ("r restore", "Restore dependencies specified in the .NET project")
            ("b build", "Builds a .NET project")
            ("p publish", "Publishes a .NET project for deployment (including the runtime)")
            ("u run", "Compiles and immediately executes a .NET project")
            ("t test", "Runs unit tests using the test runner specified in the project")
            ("k pack", "Creates a NuGet package")
            ("m migrate", "Migrates a project.json based project to a msbuild based project")
            ("c clean", "Clean build output(s)")
            ("s sln", "Modify solution (SLN) files")
            ("a add", "Add items to the project")
            ("v remove", "Remove items from the project")
            ("l list", "List items in the project")
            ("0 rerun", "Rerun last command again")
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
            | "t" | "test" -> testCommand()
            | "s" | "sln" -> slnCommand()
            | "0" | "rerun" -> 
                        match last with
                        | Some x -> x
                        | None -> Skip
            | x -> Skip

      last <- Some command

      let cmd = convertToCommandLine(command)
      match cmd with
      | Some x -> Valid(x)
      | None -> Cancel