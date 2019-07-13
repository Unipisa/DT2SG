namespace DT2SG_lib

open FSharp.Data
open FSharp.Data.CsvExtensions
open LibGit2Sharp
open System
open System.IO

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
        if not <| System.IO.Directory.Exists(dstPath) then System.IO.Directory.CreateDirectory(dstPath) |> ignore
        let srcDir = new System.IO.DirectoryInfo(srcPath)
        for file in srcDir.GetFiles() do
            let temppath = System.IO.Path.Combine(dstPath, file.Name)
            file.CopyTo(temppath, true) |> ignore
        if copySubDirs then
            for subdir in srcDir.GetDirectories() do
                let dstSubDir = System.IO.Path.Combine(dstPath, subdir.Name)
                directoryCopy subdir.FullName dstSubDir copySubDirs

    let hello name = printfn "Hello %s" name

    let initGit (root_path) =
        // find git going up
        let gitPath = Repository.Discover(root_path)
        gitPath

    let commitVersionToGit (versionPath, message, author_name, author_handle, commit_date) =
        let repo = new Repository(versionPath)
        Commands.Stage(repo, "*")
        // Create the committer's signature and commit
        let author = Signature(author_name, author_handle, commit_date)
        let committer = author
        // Commit to the repository
        let commit = repo.Commit(message, author, committer)
        ()

    let createSyntheticGit (root_path: string, metadata_path: string, ignore_path: string) =
        let git_path = initGit (root_path)
        let authorsAndDateStrings = CsvFile.Load(metadata_path).Cache()
        let IgnoreList = CsvFile.Load(ignore_path).Cache()
        let directories =
            Array.sort (Directory.GetDirectories(root_path))
            |> Array.map (fun (a: string) -> a.Substring(root_path.Length + 1, a.Length - (root_path.Length + 1)))
        for dir in directories do
            let finder = fun (row: CsvRow) -> (row.GetColumn "dir" = dir)
            let info = Seq.tryFind finder (authorsAndDateStrings.Rows)
            let dest = git_path.Replace("/.git","")
            let orig = Path.Combine(root_path, dir)
            directoryCopy orig dest true
            let none (a) = if String.IsNullOrWhiteSpace(a) then "n.d." else a
            let author_name = if info.IsSome then info.Value.GetColumn "author_name" else none (null)
            let author_handle =
                if info.IsSome then
                    //let (handle:string) = (info.Value.GetColumn "author_github") 
                    //if not(String.IsNullOrWhiteSpace(handle)) 
                    //    then none("@" + handle) 
                    //    else 
                    none ((info.Value.GetColumn "author_email"))
                else none (null)

            let message =
                if info.IsSome then none (info.Value.GetColumn "message")
                else none (null)

            let commit_date =
                if info.IsSome then
                    DateTimeOffset.ParseExact
                        (info.Value.GetColumn "date", date_format, System.Globalization.CultureInfo.InvariantCulture)
                else DateTimeOffset.Now

            Console.WriteLine
                ("Commit dir: {0} with message : {1} on {2}", orig, message, commit_date.ToLocalTime().ToString())
            commitVersionToGit
                (git_path.Replace(".git/", ""), "V" + dir + " - " + message, author_name.TrimEnd(),
                 author_handle.TrimEnd(), commit_date)
            ()
        ()