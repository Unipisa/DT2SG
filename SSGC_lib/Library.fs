namespace SSGC_lib

open System
open System.IO
open FSharp.Data
open FSharp.Data.CsvExtensions
open LibGit2Sharp
open LibGit2Sharp


//TODO: 
// *  handle and exclude symbolic links
// *  handle directories that are not versions (how? replace in each revision ?)

module Lib =
    // date format into author.csv
    let date_format = "MM'/'dd'/'yyyy HH':'mm':'ss"

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

    let commitVersionToGit(versionPath, message, author_name, author_handle, commit_date) =
        let repo = new Repository(versionPath)
        Commands.Stage(repo, "*");
        // Create the committer's signature and commit
        let author = Signature(author_name, author_handle, commit_date);
        let committer = author;
        // Commit to the repository
        let commit = repo.Commit(message, author, committer);
        ()

    let createSyntheticGit(root_path:string) =
        let gitPath = initGit(root_path);
        let csvFilename = root_path + "/../authors.csv"
        let directories = 
            Array.sort(Directory.GetDirectories(root_path)) 
            |> Array.map (fun (a:string) -> a.Substring(root_path.Length + 1 , a.Length - (root_path.Length + 1) ))
            //|> Array.filter (fuun (a:string) -> not(a = ".git")
        let authorsAndDateStrings =CsvFile.Load(csvFilename).Cache()        
        for dir in directories do
            let finder = fun (row:CsvRow) -> (row.GetColumn "dir" = dir)  
            let info =  Seq.tryFind finder (authorsAndDateStrings.Rows) 
            let dest = Path.Combine(root_path,"./../SGit/WORKBENCH_TEMPLATE/SRC/")
            let orig = Path.Combine(root_path, dir )
            directoryCopy orig dest true
            let none(a) = 
                if String.IsNullOrWhiteSpace(a) then "n.d." else a
            let author_name = if info.IsSome then info.Value.GetColumn "author_name" else none(null)
            let author_handle = 
                if info.IsSome  
                    then 
                        //let (handle:string) = (info.Value.GetColumn "author_github") 
                        //if not(String.IsNullOrWhiteSpace(handle)) 
                        //    then none("@" + handle) 
                        //    else 
                        none((info.Value.GetColumn "author_email"))
                    else none(null)
            let message = if info.IsSome then none(info.Value.GetColumn "message") else none(null)
            let commit_date =
                if info.IsSome
                    then DateTimeOffset.ParseExact(info.Value.GetColumn "date", date_format, System.Globalization.CultureInfo.InvariantCulture)
                    else DateTimeOffset.Now
            Console.WriteLine("Commit dir: {0} with message : {1}", orig, message)
            commitVersionToGit(gitPath.Replace(".git/", ""), "V" + dir + " - " + message, author_name.TrimEnd(), author_handle.TrimEnd(), commit_date)
            ()
        done

        ()
