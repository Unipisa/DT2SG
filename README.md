# DIUNIPI-SWH-DT2SG

[![Build Status](https://travis-ci.com/Unipisa/DIUNIPI-SWH-DT2SG.svg?token=uYktkpxbywknDpAJce3c&branch=master)](https://travis-ci.com/Unipisa/DIUNIPI-SWH-DT2SG)

Make a synthetic Git from directory tree 
(Directory Tree 2 Synthetic Git).

This project aim to build a tool for reconstructing a Git repository from a directory of source code:
we start from a list of directory, - at the moment - one for each release version, and we create a Git where each directory is a commit.

An auxillary csv files is used to specify authors and date of commits.

It born from the need of [DIUNIPI4SWH](https://github.com/Unipisa/DIUNIPI4SWH) for [Software Heritage](www.softwareheritage.org), partially inspired by the [Spinelli](https://www2.dmst.aueb.gr/dds/)`s work onf [unix history repository](https://github.com/dspinellis/unix-history-repo).

*The project is still in aplha stage and under development.*

## Usage

~~~
dotnet run --project ./SSGC_app/SSGC_app.fsproj $path_to_src_root
~~~

## Build

~~~
dotnet "build" "./SSGC_app/SSGC_app.fsproj" 
~~~
