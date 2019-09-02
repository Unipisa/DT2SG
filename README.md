# SWHAP-DT2SG

[![Build Status](https://travis-ci.com/Unipisa/SWHAP-DT2SG.svg?token=uYktkpxbywknDpAJce3c&branch=master)](https://travis-ci.com/Unipisa/SWHAP-DT2SG)

Make a synthetic Git from directory tree 
(Directory Tree 2 Synthetic Git).

**Documentation to be updated**

![example](./ETC/screen-commands.png)

![example](./ETC/screen.png)

![example](./ETC/screen-2.png)

This project aim to build a tool for reconstructing a Git repository from a directory of source code:
we start from a list of directory, - at the moment - one for each release version, and we create a Git where each directory is a commit.

An [auxillary csv](./metadata_example.csv) files is used to specify authors and date of commits.

It born from the need of [DIUNIPI4SWH](https://github.com/Unipisa/DIUNIPI4SWH) for [Software Heritage](www.softwareheritage.org), partially inspired by the [Spinelli](https://www2.dmst.aueb.gr/dds/)`s work onf [unix history repository](https://github.com/dspinellis/unix-history-repo).

*The project is still in aplha stage and under development.*

![example](./ETC/screen.png)

## Warning

The relased version are self-contained (git is not required), but if commits date are pre-1970, git command should be in system path.

## Usage

```bash
dotnet run --project ./SSGC_app/SSGC_app.fsproj $path_to_src_root
```

## Build

```
bash
dotnet "build" "./SSGC_app/SSGC_app.fsproj" 
```

## Release

Self-contained
```
dotnet publish --configuration Release --runtime ubuntu.18.04-x64  /p:PublishSingleFile=true --self-contained true  
./warp-packer --arch linux-x64 --input_dir bin/Release/netcoreapp2.2/ubuntu.18.04-x64/publish/ --exec DT2SG_app --output DT2SG_Ubuntu18.04-x64.exe

dotnet publish --configuration Release --runtime win-x64  /p:PublishSingleFile=true --self-contained true   
./warp-packer --arch windows-x64 --input_dir bin/Release/netcoreapp2.2/win-x64/publish/ --exec DT2SG_app.exe --output DT2SG_Win-x64.exe

dotnet publish --configuration Release --runtime osx.10.11-x64  /p:PublishSingleFile=true --self-contained true   
./warp-packer --arch macos-x64 --input_dir bin/Release/netcoreapp2.2/win-x64/publish/ --exec DT2SG_app.exe --output DT2SG_Win-x64.exe

```

```
curl -Lo warp-packer https://github.com/dgiagio/warp/releases/download/v0.3.0/linux-x64.warp-packer

chmod +x warp-packer
```