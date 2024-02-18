# CustomBuildTask
A MSBuild task that allows you to build native code cross-platformedly. 

# Features
- Cross-platform (unlike vcxproj)
- Cross compile support for:
  - Linux -> Windows (with MingW)
  - Linux -> MacOS (with osxcross)
- Supports .NET RID system for easy builds, building only what is needed.


# Usage
- Install cmake and a compiler (or more for cross-platform builds)
- Copy TestProject.Native as the starting point for your own native project.
- Read the files and their comments (specifically nuget.config and TestProject.Native.nativeproj)
- Edit the CMakeLists and add whatever projects you want
- You're done!

# Advanced
We define some variables for you to use in your CMakeLists:
- CustomBuildTaskRoot: points to the root of CustomBuildTask, for referencing .cmake files
- BUILD_PLATFORM_TARGET: OS part of the current RID, for example: "windows", "osx", "linux"
- BUILD_ARCH: Architecture part of the current RID, for example: "x64", "x86", "arm64"
- BUILD_RID: Current RID, for example: "win-x64", "linux-x64", "osx-arm64"
- NATIVE_OUTPUT_FOLDER: The path where natives should be stored (done automatically, this is for extra files that cmake doesn't place into output)