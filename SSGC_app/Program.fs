// Learn more about F# at http://fsharp.org

open System
open SSGC_lib

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    argv
        |> Array.iter (printfn "%s")

    let root_path = "./demo/cmm"//argv.[0];
    Lib.createSyntheticGit(root_path)
    
    0 // return an integer exit code
