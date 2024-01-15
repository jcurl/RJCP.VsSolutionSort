# Visual Studio Solution Sorter <!-- omit in toc -->

This project is about sorting the contents of a Visual Studio Solution File
(`.sln`) in a consistent order, that makes it easier to compare and merge.

- [1. Getting Started](#1-getting-started)
  - [1.1. Installation Requirements](#11-installation-requirements)
  - [1.2. Using `dotnet tool`](#12-using-dotnet-tool)
    - [1.2.1. From the NuGet Store](#121-from-the-nuget-store)
    - [1.2.2. From a Downloaded NuGet Package](#122-from-a-downloaded-nuget-package)
- [2. Usage Information](#2-usage-information)
  - [2.1. Getting the Version](#21-getting-the-version)
  - [2.2. Getting Command Line Help](#22-getting-command-line-help)
  - [2.3. Sorting a Solution File](#23-sorting-a-solution-file)
    - [2.3.1. Sorting a Single Solution](#231-sorting-a-single-solution)
    - [2.3.2. Sorting Multiple Solutions](#232-sorting-multiple-solutions)
      - [2.3.2.1. Controlling the Recursive Behaviour (.solutionsort file)](#2321-controlling-the-recursive-behaviour-solutionsort-file)
      - [2.3.2.2. Setting Concurrency](#2322-setting-concurrency)
    - [2.3.3. Testing Solution Sort](#233-testing-solution-sort)
- [3. Sorting Algorithm](#3-sorting-algorithm)
- [4. The Solution File Information](#4-the-solution-file-information)
  - [4.1. Nested Section Information](#41-nested-section-information)
  - [4.2. Collection of Solution Types](#42-collection-of-solution-types)
  - [4.3. Parsing the Solution File](#43-parsing-the-solution-file)

## 1. Getting Started

### 1.1. Installation Requirements

This project targets .NET Core 6.0. Ensure this is installed prior.

### 1.2. Using `dotnet tool`

You can install the build NuGet package. After installing, it is executable with
the `slnsort` executable.

#### 1.2.1. From the NuGet Store

To install from the NuGet Store:

```cmd
dotnet tool install --global RJCP.VsSolutionSort
```

To uninstall:

```cmd
dotnet tool uninstall --global RJCP.VsSolutionSort
```

#### 1.2.2. From a Downloaded NuGet Package

To install locally, assuming you've copied the file into a folder called
`nupkg`:

```cmd
dotnet tool install --global --add-source ./nupkg RJCP.VsSolutionSort
```

If you have a prerelease version (usually indicated with extra version
information after the numbers, like 1.0.0-beta):

```cmd
dotnet tool install --global --add-source ./nupkg --prerelease RJCP.VsSolutionSort
```

To uninstall:

```cmd
dotnet tool uninstall --global RJCP.VsSolutionSort
```

## 2. Usage Information

### 2.1. Getting the Version

To get the version of the program

```text
$ dotnet slnsort --version
RJCP.VsSolutionSort Version: 1.0.0-beta.20240113T185144+g1dd2665; (C) 2024, Jason Curl.
```

### 2.2. Getting Command Line Help

If you need a quick reference to using the program, run on the command line:

```text
dotnet slnsort sorts the project entries in a Visual Studio solution file in
the order as Visual Studio shows them in the Solution Explorer. Sorting the
project entries in the solution file on changes helps users compare similar
solution files, such as those often in revision control systems.

Usage:

  dotnet slnsort -?|-v
  dotnet slnsort [-d] <input.sln>
  dotnet slnsort [-d] [-j<N>] -R [<dir>]

Options:

  -? | --help
    displays this help message.
  -v | --version
    displays the version of this program.
  -d | --dryrun
    Print out the name of the file that would be processed instead of
    processing the file.
  -R | --recurse
    Search recursively from the directory given for solution files, *.sln, and
    sort them.
  -j | --jobs=<int>
    Specify the number of threads <int> that should be used when recursing.
    Default is to use the number of threads in the CPU.

Inputs:

  <input.sln> - a Visual Studio solution file.
  <dir> - the directory to search from. If this is not provided when recursing,
    the current directory is assumed.

Exit Codes:

  The following exit codes show the success of the operation.

  0 - The program ran successfully.
  1 - There was an error parsing the Visual Studio solution file.
  255 - There was an unknown error.
```

### 2.3. Sorting a Solution File

#### 2.3.1. Sorting a Single Solution

Execute the command from the terminal. Run the command once per solution file
whose contents you want to sort.

```cmd
dotnet slnsort <SolutionSort.sln>
```

On output it will *overwrite* the solution file. Ensure that there is a copy of
the file prior to running (or use your revision control system to revert in case
of fault).

#### 2.3.2. Sorting Multiple Solutions

If you have a larger project with multiple solution files, you can sort them all
recursively.

```cmd
dotnet slnsort -R
```

This will iterate from the current directory, find all solution files, and then
sort them all in place. In case of errors, the tool will try to sort as many
solutions as possible.

##### 2.3.2.1. Controlling the Recursive Behaviour (.solutionsort file)

Without any intervention, the tool will iterate all directories. In each
directory it looks for the file with the name `.solutionsort`. If this file
exists it is read for a set of inclusion and exclusion rules.

If there are no rules, the initial assumption is that all solution files in this
directory and subdirectories (unless there is a `.solutionsort` there) will not
be parsed. Usually, you should not provide a `.solutionsort` unless you want to
restrict parsing.

The contents of the `.solutionsort` file is very simple

```toml
# COMMENT
[include]
REGEX

[exclude]
REGEX
```

The regular expressions are usual .NET regular expression strings (and not file
globs). Only the file name is compared for the current folder. Directory names
are not tested.

If there are regular expressions under the `[include]` section, then the file
name must match the regular expression. If a solution does not match an entry in
the `[include]` section, it will not be parsed. If it does match, then it will
be parsed, unless there is a matching entry in the `[exclude]` section.

If there is no `[include]` section, but there is an `[exclude]` section, then it
is assumed that all entries should match, except those in the `[exclude]`
section.

The ordering of the regular expressions does not matter. If there are multiple
`[include]` or `[exclude]` sections then they are grouped as if there were only
one of each section.

##### 2.3.2.2. Setting Concurrency

On systems with very high number of CPUs, you might want to limit the
concurrency (after testing your total time). To do this, use the option `-j`
(`--jobs`) with a value indicating the number of concurrent operations the
software should use.

A table shows an example of how concurrency affects the speed of the overall
operation, when parsing on an i7-6700U with 8 hardware threads. The test was for
parsing the files (it was run in `--dryrun` so files weren't written).

| jobs | Scanning (ms) | Parsing (ms) |
| ---- | ------------- | ------------ |
| 1    | 562           | 770          |
| 2    | 340           | 453          |
| 3    | 240           | 343          |
| 4    | 187           | 282          |
| 5    | 156           | 266          |
| 6    | 141           | 240          |
| 7    | 135           | 255          |
| 8    | 125           | 234          |
| 255  | 125           | 235          |

#### 2.3.3. Testing Solution Sort

If you do not wish to sort a solution, but only confirm that the solution files
are able to be parsed, use the option `-d` (or `--dryrun`).

This will print out the actions as it is loading the solution. This is very
useful when testing the correctness of any `.solutionsort` file that might be
present.

```cmd
dotnet slnsort -dR
```

## 3. Sorting Algorithm

The projects are sorted as the Visual Studio Solution Explorer shows the
projects. That is, the projects in the solution file are now sorted by:

- Nested folder, so the high level folders are shown first; then
- If the entry is a Folder type; then
- Alphabetically

So this groups the contents of the projects in the solution file so related
projects are kept together.

The sections that are sorted are:

- The `Project`..`EndProject` elements;
- The `GlobalSection(ProjectConfigurationPlatforms)`..`EndGlobalSection`; and
- The `GlobalSection(NestedProjects)`..`EndGlobalSection`.

The existing text is maintained. The output is reformatted to UTF-8 output using
line endings native to your machine.

## 4. The Solution File Information

Microsoft describes the [Solution
File](https://learn.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2022)
so we can get to know the structure.

### 4.1. Nested Section Information

Not all information is documented, particular the section
`GlobalSection(NestedProjects)` which is used for sorting.

The section `GlobalSection(NestedProjects)` is optional. It doesn't need to be
present. if it is not present, then there is no folder structure and all
`Project` entries are shown at the top level.

If it is present, it might look like (the .. indicates unrelated missing
information):

```text
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "CPUID", "CPUID", "{25EA59ED-20C7-401C-A302-A0265E0A8F11}"
EndProject
Project("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}") = "cpuiddll", "cpuiddll.vcxproj", "{5A90E8A3-F910-4128-BCED-B2FBD9E772AD}"
EndProject
Global
  ..
  GlobalSection(NestedProjects) = postSolution
   {5A90E8A3-F910-4128-BCED-B2FBD9E772AD} = {25EA59ED-20C7-401C-A302-A0265E0A8F11}
  EndGlobalSection
EndGlobal
```

It is a list of key/value pairs, where the key is the project GUID. The value is
the *parent* in the tree structure for that project. So we can see in the
example, it has a structure of:

```text
CPUID\
 +- cpuiddll (cpuiddll.vcxproj)
```

### 4.2. Collection of Solution Types

The project only relies on the solution type
`{2150E333-8FDC-42A3-9474-1A3956D46DE8}` which is a folder. The folder type is
used to ensure that a project element can only have a folder type as its parent.

The root solution GUID is not used when sorting. These are what observed in use
(e.g. Roslyn).

| GUID                                     | Prject Type          |
| ---------------------------------------- | -------------------- |
| `{2150E333-8FDC-42A3-9474-1A3956D46DE8}` | Folder               |
| `{9A19103F-16F7-4668-BE54-9A1E7A4F7556}` | C-Sharp Project      |
| `{778DAE3C-4631-46EA-AA77-85C1314464D9}` | Visual Basic Project |
| `{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}` | C++ Project          |
| `{F2A71F9B-5D33-465A-A702-920D77279786}` | F-Sharp Project      |
| `{D954291E-2A0B-460D-934E-DC6B0785DB48}` | Shared Code Project  |

Other sites have collected a more complete list:

- [Visual Studio Project Type
  Guids](https://github.com/JamesW75/visual-studio-project-type-guid) on GitHub/
  - `{778DAE3C-4631-46EA-AA77-85C1314464D9}` isn't listed here, but observed by
    the
    [Roslyn](https://github.com/dotnet/roslyn/blob/e59fce8ec2bef59afebeabc1630f826a0ad86396/Roslyn.sln)
    project.
- [slngen Project
  GUIDs](https://github.com/microsoft/slngen/blob/main/src/Microsoft.VisualStudio.SlnGen/VisualStudioProjectTypeGuids.cs)
  from Microsoft
  - This list suggests that there's only one Folder type, and this application
    only assumes this folder type `{2150E333-8FDC-42A3-9474-1A3956D46DE8}`.

### 4.3. Parsing the Solution File

When parsing the solution file, the following restrictions are placed:

- The `Project`..`EndProject` must be in a single blob. No text in between, or
  other sections. These projects are sorted.
- There can ony be one `Global` section.
- There can only be one `GlobalSection(NestedProjects)` section. This section is
  sorted.
  - Only the format `{GUID} = {GUID}` is supported. Other lines will cause
    parsing to fail.
- There can only be one `GlobalSection(ProjectConfigurationPlatforms)` section.
  This section is sorted.
  - Only the format `{GUID}.x = config` is supported. Other lines will cause
    parsing to fail.

It will check for some errors that could occur on merging:

- If a nested entry has the same key twice.
- If a project GUID is reused
- If a project path (non solution folder) is the same (assuming that the paths
  are all normalized).
- Cyclic graphs in the nested section.
  - You'll only be told that the nested section is broken. This is because a
    cyclic entry won't be found by the root and the calculated length is
    different from the number of projects.
  - Check in Visual Studio for entries that appear in the 'root' which shouldn't
    be there.
