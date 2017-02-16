module DotConsole.Tests.Test

open NUnit.Framework
open FluentAssertions

open DotConsole.Library

[<Test>]
let shouldCreateNewCmd() =
      let project =  Console(FSharp, OutputDirectory("Hello"))
      let verb = New(project)

      let cmd = convertToCommandLine verb
      match cmd with
      | Some cmd ->
            cmd.Should().Be("dotnet new console --language F# --output Hello", "") |> ignore
      | None -> ()

