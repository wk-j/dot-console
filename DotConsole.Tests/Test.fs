module DotConsole.Tests.Test

open NUnit.Framework
open FluentAssertions

open DotConsole.Library

[<Test>]
let shouldCreateNewCmd() =
      let project =  Console(FSharp, ProjectName("Hello"), OutputDirectory(""))
      let verb = New(project)

      let cmd = verbCmd verb
      cmd.Should().Be("dotnet new console --lang F# Hello", "") |> ignore
