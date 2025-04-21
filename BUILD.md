# Build <!-- omit in toc -->

- [1. Debug Build](#1-debug-build)
- [Release Build](#release-build)
- [2. Trouble Shooting](#2-trouble-shooting)

## 1. Debug Build

To build this project, go to where the solution file is kept and run:

```sh
dotnet pack
```

## Release Build

The release build uses a Microsoft Authenticode signing certificate. This
certificate should be installed in the Windows store. Make sure that the path
points to the tool `signtool.exe`.

```cmd
$env:PATH += ";C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64"
```

You'll get errors, unless you're building from GIT with a properly labelled
repository.

```sh
dotnet pack -c Release
```

## 2. Trouble Shooting

If it can't build on Windows because files are open, ensure to terminate all
instances of `dotnet.exe` first.

```cmd
taskkill.exe /f /im dotnet.exe
```
