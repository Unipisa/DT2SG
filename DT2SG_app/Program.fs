// dotnet build
// dotnet run -r rootPath

open System
open DT2SG_lib
open CommandLine
open CommandLine.Text

type options = {
  [<Option('r', "root", Required = true, HelpText = "Path of the root of Directory Tree")>] root_string: string;
  [<Option('m', "metadata", Required = true, HelpText = "Path of the metadata file")>] metadata_string: string;
  [<Option('i', "ignore", Required = true, HelpText = "Path of the ignore list file")>] ignore_string: string;
}


[<EntryPoint>]
let main argv =

    let mutable root_path, metadata_path, ignore_path = "", "", ""
    let run (a: CommandLine.Parsed<options>) =
        root_path <- a.Value.root_string
        metadata_path <- a.Value.root_string
        ignore_path <- a.Value.root_string

    let fail a =
        Console.WriteLine(a.ToString())


    let result = CommandLine.Parser.Default.ParseArguments<options>(argv)
    match result with
        | :? Parsed<options> as parsed -> run parsed //.Value
        | :? NotParsed<options> as notParsed -> fail notParsed.Errors

    //let root_path = "./demo/cmm"//argv.[0];
    //let root_path = argv.[0];

    Lib.createSyntheticGit(root_path, metadata_path, ignore_path)
    Console.WriteLine("You Git is in " + root_path + "/SGit/.git")

    0 // return an integer exit code
