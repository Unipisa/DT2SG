// Learn more about F# at http://fsharp.org

open System
open SSGC_lib

[<EntryPoint>]
let main argv =
    //let root_path = "./demo/cmm"//argv.[0];
    let root_path = argv.[0];
    Lib.createSyntheticGit(root_path)
    Console.WriteLine("You Git is in " + root_path + "/SGit/.git")
    
    0 // return an integer exit code
