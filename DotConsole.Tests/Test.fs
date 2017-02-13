module DotConsole.Tests.Test

open NUnit.Framework
open FluentAssertions

open DotConsole.Library

[<Test>]
let shouldCreateNewCmd() =
      let project =  Console(FSharp, OutputDirectory("Hello"))
      let verb = New(project)

      let cmd = verbCmd verb
      cmd.Should().Be("dotnet new console --language F# --output Hello", "") |> ignore
