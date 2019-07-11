﻿// dotnet build
// dotnet run -r rootPath

open System
open SSGC_lib
open CommandLine
open CommandLine.Text

type options = {
  [<Option('r', "root", Required = true, HelpText = "Path of the root of Directory Tree")>] root_string : string;
  //[<Option(HelpText = "Prints all messages to standard output.")>] verbose : bool;
  //[<Option(Default = "русский", HelpText = "Content language.")>] language : string;
  //[<Value(0, MetaName="offset", HelpText = "File offset.")>] offset : int64 option;
}


[<EntryPoint>]
let main argv =

    let mutable root_path = ""
    let run (a:CommandLine.Parsed<options>) = 
        root_path <- a.Value.root_string 
        //Console.WriteLine(a.Value.ToString())

    let fail a = 
        Console.WriteLine(a.ToString())


    let result = CommandLine.Parser.Default.ParseArguments<options>(argv)
    match result with
        | :? Parsed<options> as parsed ->  run parsed //.Value
        | :? NotParsed<options> as notParsed -> fail notParsed.Errors

    //let root_path = "./demo/cmm"//argv.[0];
    //let root_path = argv.[0];
   
    Lib.createSyntheticGit(root_path)
    Console.WriteLine("You Git is in " + root_path + "/SGit/.git")
    
    0 // return an integer exit code
