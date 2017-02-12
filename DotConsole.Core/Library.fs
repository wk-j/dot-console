
module DotConsole.Core.Library

type SlnName = string
type ProjName = string

type SlnVerb =
      | New of SlnName
      | Add of SlnName * ProjName
      | Remove of SlnName * ProjName

 
type New =
      | Sln of SlnVerb 
      | Proj 

