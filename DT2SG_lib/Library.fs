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
        let repo = new Repository(gitPath)
        let branches = repo.Branches
        let mutable exist_SRC_branch = false;
        let mutable src_branch = repo.Head
        for b in branches do 
         if b.FriendlyName = "SRC" 
            then
                exist_SRC_branch <- true
                src_branch <- b
        done
        if not(exist_SRC_branch) 
            then
                src_branch <- repo.CreateBranch("SRC")
        //Commands.Checkout(repo , src_branch) |> ignore
        repo


    let commitVersionToGit (repo:Repository, message, author_name, author_email, author_date, tag, committer_name, committer_email, commit_date) =
        //let repo = new Repository(versionPath)
        //let branch = repo.CreateBranch("SRC");
        let src_branch = repo.Branches.["SRC"]
        Commands.Checkout(repo , src_branch) |> ignore
        Commands.Stage(repo, "*")
        // Create the committer's signature and commit
        let author = Signature(author_name, author_email, author_date)
        let committer = Signature(committer_name, committer_email, commit_date)
        // Commit to the repository
        let commit = repo.Commit(message, author, committer)
        let tag = repo.ApplyTag(tag);
        ()

    let after_latest_slash (dir: string) =
                        let temp = dir.Split('/')
                        temp.[temp.Length - 1]


    let createSyntheticGit (root_path: string, metadata_path: string, ignore_path: string) =
        let git = initGit (root_path)
        let git_path = git.Info.Path 
        let authorsAndDateStrings = CsvFile.Load(metadata_path).Cache()
        let IgnoreList = CsvFile.Load(ignore_path).Cache().Rows
        let filter_ignore_dir = fun (row: string) ->
                                    not (Seq.exists (fun (ignore_row: CsvRow) ->
                                                        let dir = after_latest_slash row
                                                        dir = (ignore_row.Columns.[0])
                                                    )
                                IgnoreList)
        let directories =
                Array.sort (Directory.GetDirectories(root_path))
                |> Array.filter filter_ignore_dir
        for dir in directories do
            let short_dir = after_latest_slash dir
            let finder = fun (row: CsvRow) -> 
                            (row.GetColumn "dir" = short_dir)
            let info = Seq.tryFind finder (authorsAndDateStrings.Rows)
            let dest = git_path.Replace("/.git", "")
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

            let author_date =
                if info.IsSome then
                    DateTimeOffset.ParseExact
                        (info.Value.GetColumn "date", date_format, System.Globalization.CultureInfo.InvariantCulture)
                else DateTimeOffset.Now

            let tag = short_dir


            let commiter_name = "Guido Scatena"
            let committer_email = "guido.scatena@unipi.it"
            let commit_date = DateTimeOffset.Now
            Console.WriteLine
                ("Commit dir: {0} with message : {1} on {2}", orig, message, commit_date.ToLocalTime().ToString())
            commitVersionToGit
                (git,
                 short_dir + " - " + message,
                 author_name.TrimEnd(),
                 author_handle.TrimEnd(),
                 author_date,
                 tag,
                 commiter_name,
                 committer_email,
                 commit_date
                    )
            ()
        ()