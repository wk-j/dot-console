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

