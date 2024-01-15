# Build Instructions <!-- omit in toc -->

This was build with the .NET 8.0 SDK and a connection to the Internet for the
NuGet packages.

- [1. Building](#1-building)
- [2. Creating the NuGet Package](#2-creating-the-nuget-package)
  - [2.1. Install Locally for Testing](#21-install-locally-for-testing)
  - [2.2. Uninstalling](#22-uninstalling)

## 1. Building

To build the package:

```cmd
$ cd SolutionSort
$ dotnet build
```

## 2. Creating the NuGet Package

To build the NuGet package

```cmd
$ cd SolutionSort
$ dotnet -c Release pack
```

The output is in the `nupkg` folder.

### 2.1. Install Locally for Testing

```cmd
dotnet tool install --global --add-source ./nupkg RJCP.VsSolutionSort
```

If you're building a prerelease

```cmd
dotnet tool install --global --add-source ./nupkg --prerelease RJCP.VsSolutionSort
```

### 2.2. Uninstalling

```cmd
dotnet tool uninstall --global RJCP.VsSolutionSort
```
