namespace SSGC_lib

open System
open System.IO
open FSharp.Data
open FSharp.Data.CsvExtensions
open LibGit2Sharp

module Lib =

    let rec directoryCopy srcPath dstPath copySubDirs =

        if not <| System.IO.Directory.Exists(srcPath) then
            let msg = System.String.Format("Source directory does not exist or could not be found: {0}", srcPath)
            raise (System.IO.DirectoryNotFoundException(msg))

        if not <| System.IO.Directory.Exists(dstPath) then
            System.IO.Directory.CreateDirectory(dstPath) |> ignore

        let srcDir = new System.IO.DirectoryInfo(srcPath)

        for file in srcDir.GetFiles() do
            let temppath = System.IO.Path.Combine(dstPath, file.Name)
            file.CopyTo(temppath, true) |> ignore

        if copySubDirs then
            for subdir in srcDir.GetDirectories() do
                let dstSubDir = System.IO.Path.Combine(dstPath, subdir.Name)
                directoryCopy subdir.FullName dstSubDir copySubDirs

    let hello name =
        printfn "Hello %s" name
          
    let initGit(path)   =
        Directory.CreateDirectory(path + "/../SGit") |> ignore
        let gitPath = Repository.Init(path + "/../SGit")
        gitPath

    let commitVersionToGit(versionPath) =
        let repo = new Repository(versionPath)
        Commands.Stage(repo, "*");
        // Create the committer's signature and commit
        let author = new Signature("James", "@jugglingnutcase", DateTimeOffset.Now);
        let committer = author;
        // Commit to the repository
        let commit = repo.Commit("Here's a commit i made!", author, committer);
        ()

    let createSyntheticGit(root_path:string) =
        //let pp = Directory.GetCurrentDirectory()
        //let np = pp + "/" + root_path
        //Directory.SetCurrentDirectory(root_path)
        let gitPath = initGit(root_path);
        let csvFilename = root_path + "/../authors.csv"
        let directories = 
            Array.sort(Directory.GetDirectories(root_path)) 
            |> Array.map (fun (a:string) -> a.Substring(root_path.Length + 1 , a.Length - (root_path.Length + 1) ))
        let authorsAndDateStrings =CsvFile.Load(csvFilename).Cache()        
        for dir in directories do
            let finder = fun (row:CsvRow) -> (row.GetColumn "dir" = dir)  
            let info =  Seq.tryFind finder (authorsAndDateStrings.Rows) 
            let dest = Path.Combine(root_path,"./../SGit/")
            let orig = Path.Combine(root_path, dir )
            directoryCopy orig dest true
            commitVersionToGit(gitPath)
            //TODO: passare info autore e data
            //TODO: remove direcotry
            ()
        done

        ()
