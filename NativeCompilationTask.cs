using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CustomBuildTask;

public class NativeCompilationTask : Microsoft.Build.Utilities.Task
{
    public enum ETargetOS : int {
        Invalid,
        Windows, 
        MacOS,
        Linux
    }

    public enum ETargetArch : int {
        Invalid,
        X64,
        X86,
        ARM64
    }

    public struct BuildRequest {
        public ETargetArch Arch;
        public ETargetOS OS;
        public readonly string RID;
        public readonly string OSStr => GetStringFromOS(this.OS);
        public readonly string ArchStr => GetStringFromArch(this.Arch);

        public BuildRequest(string rid) {
            (string os, string arch) = RIDParser.Parse(rid);
            this.Arch = GetArchFromString(arch);
            this.OS = GetOSFromString(os);
            this.RID = rid;
        }

        public BuildRequest(BuildRequest other) {
            this.Arch = other.Arch;
            this.OS = other.OS;
            this.RID = other.RID;
        }
    }

    public static readonly ETargetOS CurrentOS;

    [Required]
    public string RootDir { get; set; } = "";

    static NativeCompilationTask() {
        if (OperatingSystem.IsWindows()) {
            CurrentOS = ETargetOS.Windows;
        } else if (OperatingSystem.IsLinux()) {
            CurrentOS = ETargetOS.Linux;
        } else if (OperatingSystem.IsMacOS()) {
            CurrentOS = ETargetOS.MacOS;
        } else {
            throw new NotSupportedException("Your OS is unsupported");
        }
    }

    public string TargetRID { get; set; } = "";

    [Required]
    public string OutputDir { get; set; } = "";

    [Required]
    public string IntermediateOutputDir { get; set; } = "";

    [Required]
    public string CMakeBuildConfig { get; set; } = "";

    [Required]
    public string CustomBuildTaskRoot { get; set; } = "";
    public string CMakeBuildAllArches { get; set; } = "";
    public string BuildForRuntimeIdentifiers { get; set; } = "";

    public override bool Execute()
    {
        List<string> targetRIDS = [];

        if (!FileEx.Exists("cmake")) {
            Log.LogError("Cannot build natives without CMake. Please install cmake and add it to PATH");
            return false;
        }

        bool hasTargetRID = !string.IsNullOrEmpty(TargetRID);

        if (string.IsNullOrEmpty(OutputDir)) {
            Log.LogError("Can't build natives with empty OutputPath");
            return false;
        }

        if (string.IsNullOrEmpty(IntermediateOutputDir)) {
            Log.LogError("Can't build natives with empty IntermediateOutputPath");
            return false;
        }

        if (string.IsNullOrEmpty(CustomBuildTaskRoot)) {
            Log.LogError("Can't build natives with empty CustomBuildTaskRoot");
            return false;
        }

        if (string.IsNullOrEmpty(CMakeBuildConfig)) {
            Log.LogWarning("CMakeBuildConfig not specified, defaulting to release! Please specify CMakeBuildConfig property.");
            CMakeBuildConfig = "Release";
        }

        bool hasMultiRID = !string.IsNullOrEmpty(BuildForRuntimeIdentifiers);

        if (!hasMultiRID && !hasTargetRID) {
            Log.LogError("Can't build natives without specifying RuntimeIdentifier or BuildForRuntimeIdentifiers");
            return false;
        }
        
        if (hasMultiRID) {
            targetRIDS.AddRange(BuildForRuntimeIdentifiers.Split(";"));
        } else {
            targetRIDS.Add(TargetRID);
        }

        if (!hasMultiRID && !CanCompileFor(new BuildRequest(targetRIDS.First()).OS)) {
            Log.LogError("Can't build natives for target " + targetRIDS.First() + ", missing compiler!");
            return false;
        }

        List<BuildRequest> requests = [];
        foreach (var rid in targetRIDS)
        {
            var request = new BuildRequest(rid);
            if (CanCompileFor(request.OS)) {
                if (!string.IsNullOrEmpty(CMakeBuildAllArches))
                {
                    foreach (var arch in CMakeBuildAllArches.Split(';'))
                    {
                        requests.Add(new BuildRequest(request)
                        {
                            Arch = GetArchFromString(arch)
                        });
                    }
                } else {
                    requests.Add(request);
                }
            } else {
                Log.LogWarning("Not building natives for " + rid + ", missing compiler!");
            }
        }

        try
        {
            foreach (var item in requests)
            {
                this.Compile(item);
            }
        }
        catch (CompileException e)
        {
            Log.LogError("Building natives failed with error " + e.Message);
            return false;
        }

        return true;
    }

    private static ETargetArch GetArchFromString(string arch) {
        return arch switch
        {
            "x64" => ETargetArch.X64,
            "x86" => ETargetArch.X86,
            "arm64" => ETargetArch.ARM64,
            _ => throw new ArgumentOutOfRangeException(nameof(arch), "Invalid arch string " + arch),
        };
    }

    private static ETargetOS GetOSFromString(string os) {
        return os switch
        {
            "win" => ETargetOS.Windows,
            "osx" => ETargetOS.MacOS,
            "linux" => ETargetOS.Linux,
            _ => throw new ArgumentOutOfRangeException(nameof(os), "Invalid os string " + os),
        };
    }

    private static string GetStringFromArch(ETargetArch arch) {
        return arch switch
        {
            ETargetArch.X64 => "x64",
            ETargetArch.X86 => "x86",
            ETargetArch.ARM64 => "arm64",
            _ => throw new ArgumentOutOfRangeException(nameof(arch), "Invalid arch enum " + arch),
        };
    }

    private static string GetStringFromOS(ETargetOS os) {
        return os switch
        {
            ETargetOS.Windows => "win",
            ETargetOS.MacOS => "osx",
            ETargetOS.Linux => "linux",
            _ => throw new ArgumentOutOfRangeException(nameof(os), "Invalid os enum " + os),
        };
    }

    private static bool CanCompileFor(ETargetOS targetOS) {
        switch (targetOS)
        {
            case ETargetOS.Windows:
                if (CurrentOS == ETargetOS.Linux || CurrentOS == ETargetOS.MacOS) {
                    return FileEx.Exists("x86_64-w64-mingw32-gcc") && FileEx.Exists("ldd");
                } else if (CurrentOS == ETargetOS.Windows) {
                    // Assume the user has done their due diligence and installed a compiler. TODO: test for compilers with CMake
                    return true;
                }
                break;
            case ETargetOS.Linux:
                if (CurrentOS == ETargetOS.Linux || CurrentOS == ETargetOS.MacOS) {
                    return (FileEx.Exists("g++") || FileEx.Exists("gcc") || FileEx.Exists("ninja")) && FileEx.Exists("ldd");
                }
                break;
            case ETargetOS.MacOS:
                if (CurrentOS == ETargetOS.Linux) {
                    return FileEx.Exists("osxcross-conf");
                }

                if (CurrentOS == ETargetOS.MacOS) {
                    return FileEx.Exists("clang") || FileEx.Exists("gcc");
                }
                break;
        }

        return false;
    }

    private static string GetOSXCrossVar(string key) {
        var osxcrossConfPath = FileEx.GetFullPath("osxcross-conf");
        if (osxcrossConfPath == null) {
            throw new Exception("osxcross not installed");
        }

        Process proc = new()
        {
            StartInfo = new()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = osxcrossConfPath
            }
        };

        proc.Start();

        string? lastLine;
        while (!proc.StandardOutput.EndOfStream) 
        {
            lastLine = proc.StandardOutput.ReadLine();
            if (lastLine != null) {
                string targetStr = $"export {key}=";
                if (lastLine.Contains(targetStr)) {
                    proc.Kill();
                    return lastLine.Remove(0, targetStr.Length);
                }
            }
        }

        proc.WaitForExit();
        throw new Exception("Getting OSXCross version failed");
    }

    private void Compile(BuildRequest request) {
        bool isCrossCompile = request.OS != CurrentOS;
        this.Log.LogMessage(MessageImportance.High, $"Attempting to build natives for '{request.RID}', with CMake configuration {CMakeBuildConfig}");
        this.Log.LogMessage(MessageImportance.High, $"IsCrossCompile: '{isCrossCompile}'");
        
        string builddir = Path.Combine(IntermediateOutputDir, request.RID);
        string outputdir = Path.Combine(OutputDir, request.RID, "native");
        Directory.CreateDirectory(builddir);
        Directory.CreateDirectory(outputdir);
        string compilerIdentity = "";
        string cmakeConfigureFlags = "";

        switch (request.OS)
        {
            case ETargetOS.Windows:
                if (CurrentOS == ETargetOS.Linux) {
                    if (request.Arch == ETargetArch.X64) {
                        compilerIdentity = "MingW64";
                    } else if (request.Arch == ETargetArch.X86) {
                        compilerIdentity = "MingW32";
                    } else if (request.Arch == ETargetArch.ARM64) {
                        compilerIdentity = "MingWARM64";
                    } else {
                        throw new InvalidOperationException("Arch " + request.Arch + " not supported for cross compile for Linux -> Windows");
                    }
                }

                if (CurrentOS == ETargetOS.Windows) {
                    if (request.Arch == ETargetArch.X86) {
                        cmakeConfigureFlags = "-A Win32";
                    }
                }
                break;

            case ETargetOS.Linux:
                if (CurrentOS == ETargetOS.Linux) {
                    if (request.Arch == ETargetArch.X86) {
                        compilerIdentity = "GCC32";
                    }
                }
                break;

            case ETargetOS.MacOS:
                if (CurrentOS == ETargetOS.Linux) {
                    // Use osxcross for everything
                    string osxcrossversion = GetOSXCrossVar("OSXCROSS_TARGET");
                    string osxcrosstarget = Path.GetFullPath(GetOSXCrossVar("OSXCROSS_TARGET_DIR"));
                    cmakeConfigureFlags = $"-DOSXCROSS_TARGET={osxcrossversion} -DOSXCROSS_TARGET_DIR=\"{osxcrosstarget}\" ";
                    compilerIdentity = "osxcross";
                }
                break;
        }

        // Set the toolchain file if we need one
        if (!string.IsNullOrEmpty(compilerIdentity)) {
            cmakeConfigureFlags += $" -DCMAKE_TOOLCHAIN_FILE=\"{CustomBuildTaskRoot}/crosscomp/{compilerIdentity}.cmake\"";
        }

        this.Log.LogMessage(MessageImportance.High, $"Building {request.RID} natives " + (string.IsNullOrEmpty(compilerIdentity) ? "" : "with " + compilerIdentity));
        this.RunCMake($"\"{RootDir}\" {cmakeConfigureFlags} -DCustomBuildTaskRoot=\"{CustomBuildTaskRoot}\" -DBUILD_PLATFORM_TARGET=\"{request.OSStr}\" -DBUILD_ARCH=\"{request.ArchStr}\" -DBUILD_RID=\"{request.RID}\" -DNATIVE_OUTPUT_FOLDER=\"{outputdir}\"", builddir);
        this.RunCMake($"--build . --config {CMakeBuildConfig} --parallel {Environment.ProcessorCount*2}", builddir);        
    }

    public void RunCMake(string args, string builddir) {
        Process proc = new()
        {
            StartInfo = new("cmake", args)
            {
                WorkingDirectory = builddir,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            }
        };

        this.Log.LogCommandLine("cmake " + args);
        proc.OutputDataReceived += CMakeOutputHandler;
        proc.ErrorDataReceived += CMakeOutputHandler;
        proc.Start();
        proc.BeginErrorReadLine();
        proc.BeginOutputReadLine();
        proc.WaitForExit();
        if (proc.ExitCode != 0) {
            throw new CompileException("cmake exited with error " + proc.ExitCode);
        }
    }

    private void CMakeOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            this.Log.LogMessage(MessageImportance.High, outLine.Data);
        //     if (TryGetErrorInfoFromLine(outLine.Data, out string warningCode, out string file, out int lineNumber, out string message, out bool isError)) {
        //         if (isError) {
        //             LogWarning("", warningCode, "", file, lineNumber, 0, 0, 0, message);
        //         } else {
        //             LogWarning("", warningCode, "", file, lineNumber, 0, 0, 0, message);
        //         }
        //     } else {
        //         LogMessage("", "", "", "", 0, 0, 0, 0, MessageImportance.High, outLine.Data);
        //     }
        }
    }

    //TODO: make this work one day to get correct error codes and whatnot showing
    private bool TryGetErrorInfoFromLine(string line, out string warningCode, out string file, out int lineNumber, out string message, out bool isError) {
        warningCode = "";
        file = "";
        lineNumber = 0;
        message = "";
        isError = false;
        return false;
    }
}