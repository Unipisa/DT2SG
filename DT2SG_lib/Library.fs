﻿namespace DT2SG_lib

open FSharp.Data
open FSharp.Data.CsvExtensions
open LibGit2Sharp
open System
open System.IO

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

    let initGit (root_path,branch_name,committer_name, committer_email) =
        // find git going up
        let gitPath = Repository.Discover(root_path)
        let repo = 
            if isNull(gitPath) then new Repository(root_path) else new Repository(gitPath)
        let branches = repo.Branches
        let mutable exist_SRC_branch = false;
        let mutable src_branch = repo.Head
        for b in branches do
         if b.FriendlyName = branch_name
            then
                exist_SRC_branch <- true
                src_branch <- b
        done
        if not(exist_SRC_branch)
            then
                let master = repo.Branches.["master"]
                let emptyCommit = Array.empty<Commit>
                let treeDefinition = new TreeDefinition()
                let tree = repo.ObjectDatabase.CreateTree(treeDefinition)
                let empty_sig = new Signature(committer_name, committer_email, DateTimeOffset.Now )
                let commit = repo.ObjectDatabase.CreateCommit(empty_sig, empty_sig, "empty commit", tree, emptyCommit, false)
                src_branch <- repo.Branches.Add(branch_name, commit)
        repo

    let commitVersionToGit (
                                existing_files: Set<string>, 
                                dir_to_add: string, 
                                repo: Repository, 
                                message, 
                                author_name, 
                                author_email, 
                                author_date, 
                                tag, 
                                committer_name, 
                                committer_email, 
                                commit_date, 
                                ignore_files_to_commit,
                                branch_name,
                                relative_src_path,
                                is_first_commit:bool
                                ) =
        let filter_existing_files = (fun (file: string) -> not(existing_files.Contains(file)))
        let mutable src_branch = repo.Branches.[branch_name]

        //qui "source" in realtà è path indicato - git path
        Commands.Checkout(repo, src_branch) |> ignore
        let options = new CheckoutOptions()
        options.CheckoutModifiers <- CheckoutModifiers.Force // { CheckoutModifiers = CheckoutModifiers.Force };
        repo.CheckoutPaths("master", [relative_src_path + dir_to_add], options);
        if not(Seq.isEmpty ignore_files_to_commit)
            then
                for file in ignore_files_to_commit do
                    repo.CheckoutPaths("master", [relative_src_path + file], options)
                    if System.IO.File.Exists(repo.Info.WorkingDirectory + relative_src_path + file) 
                        then System.IO.File.Copy(repo.Info.WorkingDirectory + relative_src_path + file, repo.Info.WorkingDirectory + "/" + file )
                        else directoryCopy (repo.Info.WorkingDirectory + relative_src_path + file) (repo.Info.WorkingDirectory) true
                done
        directoryCopy (repo.Info.WorkingDirectory + relative_src_path + dir_to_add) (repo.Info.WorkingDirectory) true
        let files_unstage =
                    Directory.GetFiles (repo.Info.WorkingDirectory + relative_src_path, "*.*", SearchOption.AllDirectories)
        System.IO.Directory.Delete(repo.Info.WorkingDirectory + relative_src_path, true)
        //for f in  existing_files do  repo.Index.Remove(f) done

        let files =
                    Directory.GetFiles (repo.Info.WorkingDirectory, "*.*", SearchOption.AllDirectories)
                    //|> Array.filter filter_existing_files
        //repo.Stage (files);
        Commands.Unstage(repo, files_unstage)
        Commands.Stage(repo, files)
        // Create the committer's signature and commit
        let author = Signature(author_name, author_email, author_date)
        let committer = Signature(committer_name, committer_email, commit_date)
        // Commit to the repository
        if is_first_commit
           then 
                let emptyCommit = Array.empty<Commit>
                let treeDefinition = new TreeDefinition()
                let tree = repo.ObjectDatabase.CreateTree(treeDefinition)
                let last_commit = repo.ObjectDatabase.CreateCommit(author, committer, message, tree, emptyCommit, false)
                //let last_commit = repo.Commit(message, author, committer) 
                let master_branch =  repo.Branches.["master"]
                Commands.Checkout(repo, master_branch) |> ignore
                repo.Branches.Remove(src_branch)
                src_branch <- repo.Branches.Add(branch_name, last_commit)
                Commands.Checkout(repo, src_branch) |> ignore
            else
                let last_commit = repo.Commit(message, author, committer)
                () 
        let tag = repo.ApplyTag(tag);
        ()

    let after_latest_slash (dir: string) =
                        let temp = dir.Split('/')
                        temp.[temp.Length - 1]


    let createSyntheticGit (root_path: string, metadata_path: string, ignore_path: string, committer_name, committer_email) =
        let branch_name = "src"
        let git = initGit (root_path, branch_name, committer_name, committer_email)
        let existing_files =
                    Directory.GetFiles (git.Info.WorkingDirectory, "*.*", SearchOption.AllDirectories)
                    |> Set.ofArray

        let git_path = git.Info.Path
        let relative_src_path = root_path.Replace(git_path.Replace("/.git", ""), "")
        let authorsAndDateStrings = CsvFile.Load(metadata_path).Cache()
        let IgnoreList = CsvFile.Load(ignore_path).Cache().Rows
        let ignore_files = Seq.map (fun (ignore_row: CsvRow) -> ignore_row.Columns.[0]) IgnoreList
        let filter_ignore_dir = fun (row: string) ->
                                    not (Seq.exists (fun (ignore_row: CsvRow) ->
                                                        let dir = after_latest_slash row
                                                        dir = (ignore_row.Columns.[0])
                                                    )
                                IgnoreList)
        let directories =
                Array.sort (Directory.GetDirectories(root_path))
                |> Array.filter filter_ignore_dir
        let last_dir = after_latest_slash (Array.last directories)
        let mutable is_first_commit = true
        for dir in directories do
            let short_dir = after_latest_slash dir
            let finder = fun (row: CsvRow) ->
                            (row.GetColumn "dir" = short_dir)
            let info = Seq.tryFind finder (authorsAndDateStrings.Rows)
            let dest = git_path.Replace("/.git", "")
            let orig = Path.Combine(root_path, dir)
            //directoryCopy orig dest true
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

            let ignore_files_to_commit =
                if short_dir  = last_dir
                    then ignore_files
                    else Seq.empty

            let commit_date = DateTimeOffset.Now
            Console.WriteLine
                ("Commit dir: {0} with message : {1} on {2}", orig, message, commit_date.ToLocalTime().ToString())
            commitVersionToGit
                (
                 existing_files,
                 short_dir,
                 git,
                 short_dir + " - " + message,
                 author_name.TrimEnd(),
                 author_handle.TrimEnd(),
                 author_date,
                 tag,
                 committer_name,
                 committer_email,
                 commit_date,
                 ignore_files_to_commit,
                 branch_name,
                 relative_src_path,
                 is_first_commit
                    )
            is_first_commit <- false
            ()
        ()