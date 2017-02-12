module DotConsole.Library

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

let inline value x : string = GetValue $ x

type Language =
      | FSharp
      | CSharp

type Project =
      | Console of Language * ProjectName * OutputDirectory
      | ClassLib of Language * ProjectName * OutputDirectory
      | MsTest of Language * ProjectName * OutputDirectory
      | XUnit of Language * ProjectName * OutputDirectory
      | Web of  ProjectName * OutputDirectory
      | Mvc of Language * ProjectName * OutputDirectory
      | WebApi of ProjectName * OutputDirectory
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
      | FSharp -> "--lang F#"
      | CSharp -> "--lange C#"

let projectCmd project =

      let gen types proj lang =
            let path = value proj 
            "{type} {lange} {name}"
                  .Replace("{type}", types)
                  .Replace("{lange}", langCmd lang)
                  .Replace("{name}", path)

      match project with
      | Console (lang, proj, out)   -> gen "console" proj lang
      | ClassLib (lang, proj, out)  -> gen "classlib" proj lang
      | MsTest (lang, proj, out)    -> gen "mstest" proj lang
      | XUnit (lang, proj, out)     -> gen "xunit" proj lang 
      | X                           -> ""

let verbCmd verb =
      match verb with
      | New project -> sprintf "dotnet new %s" (projectCmd project)
      | Add (path, reference) -> ""
      | List (path, listType) -> ""

