# Visual Studio Solution Sorter <!-- omit in toc -->

This project is about sorting the contents of a Visual Studio Solution File
(`.sln`) in a consistent order, that makes it easier to compare and merge.

- [1. Getting Started](#1-getting-started)
  - [1.1. Installation Requirements](#11-installation-requirements)
- [2. Usage Information](#2-usage-information)
  - [2.1. Getting the Version](#21-getting-the-version)
  - [2.2. Getting Command Line Help](#22-getting-command-line-help)
  - [2.3. Sorting a Solution File](#23-sorting-a-solution-file)
- [3. Sorting Algorithm](#3-sorting-algorithm)
- [4. The Solution File Information](#4-the-solution-file-information)
  - [4.1. Nested Section Information](#41-nested-section-information)
  - [4.2. Collection of Solution Types](#42-collection-of-solution-types)
  - [4.3. Parsing the Solution File](#43-parsing-the-solution-file)

## 1. Getting Started

### 1.1. Installation Requirements

This project targets .NET Core 6.0. Ensure this is installed prior.

## 2. Usage Information

### 2.1. Getting the Version

To get the version of the program

```text
$ VsSolutionSort.exe --version
VsSolutionSort Version: 1.0.0-beta.20240113T185144+g1dd2665; (C) 2024, Jason Curl.
```

### 2.2. Getting Command Line Help

If you need a quick reference to using the program, run on the command line:

```text
$ VsSolutionSort.exe --help
VsSolutionSort sorts the project entries in a Visual Studio solution file in the order as Visual Studio
shows them in the Solution Explorer. Sorting the project entries in the solution file on changes helps users
compare similar solution files, such as those often in revision control systems.

Usage:

  VsSolutionSort.exe [options] <input.sln>

Options:

  -? | --help
    displays this help message
  -v | --version
    displays the version of this program

Inputs Files:

  <input.sln> - a Visual Studio solution file.

Exit Codes:

  The following exit codes show the success of the operation.

  0 - The program ran successfully
  1 - There was an error parsing the Visual Studio solution file
  255 - There was an unknown error
```

### 2.3. Sorting a Solution File

Execute the command from the terminal. Run the command once per solution file
whose contents you want to sort.

```cmd
VsSolutionSort.exe <SolutionSort.sln>
```

On output it will *overwrite* the solution file. Ensure that there is a copy of
the file prior to running (or use your revision control system to revert in case
of fault).

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
