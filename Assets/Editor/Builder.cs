using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;
using System;
using System.Linq;
using System.Text;

public class Builder
{
    static BuildOptions _cliBuildOptions = BuildOptions.None;

    static string[] GetEnabledScenes()
    {
        string[] excludedPatterns = { "TestScene" };
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled && !string.IsNullOrEmpty(scene.path))
            .Select(scene => scene.path)
            .Where(path => !excludedPatterns.Any(pattern => path.Contains(pattern)))
            .ToArray();
    }

    static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == $"-{name}" && i + 1 < args.Length)
                return args[i + 1];
        }
        return null;
    }

    static bool? ParseBool(string val)
    {
        if (val == null) return null;
        if (val.Equals("true", StringComparison.OrdinalIgnoreCase) || val == "1") return true;
        if (val.Equals("false", StringComparison.OrdinalIgnoreCase) || val == "0") return false;
        return null;
    }

    static void PrintCurrentSettings(string title)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{title} ==========");
        sb.AppendLine($"- emscriptenArgs              = {PlayerSettings.WebGL.emscriptenArgs}");
        sb.AppendLine($"- il2cppCodegen               = {PlayerSettings.GetIl2CppCodeGeneration(NamedBuildTarget.WebGL)}");
        sb.AppendLine($"- compressionFormat           = {PlayerSettings.WebGL.compressionFormat}");
        sb.AppendLine($"- managedStrippingLevel       = {PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.WebGL)}");
        sb.AppendLine($"- il2cppCompilerConfiguration = {PlayerSettings.GetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL)}");
        sb.AppendLine($"- stripEngineCode             = {PlayerSettings.stripEngineCode}");
        sb.AppendLine($"- stripUnusedMeshComponents   = {PlayerSettings.stripUnusedMeshComponents}");
        sb.AppendLine($"- strictShaderVariantMatching = {PlayerSettings.strictShaderVariantMatching}");
        sb.AppendLine($"- bakeCollisionMeshes         = {PlayerSettings.bakeCollisionMeshes}");
        sb.AppendLine($"- WebGLExceptionSupport       = {PlayerSettings.WebGL.exceptionSupport}");
        sb.AppendLine($"- gcIncremental               = {PlayerSettings.gcIncremental}");
        sb.AppendLine($"- webGLBuildSubtarget         = {EditorUserBuildSettings.webGLBuildSubtarget}");
        sb.AppendLine($"- development                 = {EditorUserBuildSettings.development}");
        sb.AppendLine($"- connectProfiler             = {EditorUserBuildSettings.connectProfiler}");
        sb.AppendLine($"- allowDebugging              = {EditorUserBuildSettings.allowDebugging}");
        sb.AppendLine($"- cliBuildOptions             = {_cliBuildOptions}");
        sb.AppendLine("===================================");

        Debug.Log(sb.ToString());
    }

    static void ApplySettingsFromArgs()
    {
        // CLI: -webGLBuildSubtarget Generic|DXT|ETC2|ASTC
        string webGLBuildSubtarget = GetArg("webGLBuildSubtarget");
        if (!string.IsNullOrEmpty(webGLBuildSubtarget) &&
            Enum.TryParse(webGLBuildSubtarget, true, out WebGLTextureSubtarget subtarget))
        {
            EditorUserBuildSettings.webGLBuildSubtarget = subtarget;
        }

        // CLI: -webglOptimize O0|O1|O2|O3|Oz (mapped to emscripten optimization flags)
        string optLevel = GetArg("webglOptimize");
        if (!string.IsNullOrEmpty(optLevel))
        {
            PlayerSettings.WebGL.emscriptenArgs = $"-{optLevel} -s";
        }

        // CLI: -compressionFormat Gzip|Brotli|Disabled or 0|1|2
        string compressionRaw = GetArg("compressionFormat");
        if (!string.IsNullOrEmpty(compressionRaw))
        {
            if (int.TryParse(compressionRaw, out int compressionInt) &&
                Enum.IsDefined(typeof(WebGLCompressionFormat), compressionInt))
            {
                PlayerSettings.WebGL.compressionFormat = (WebGLCompressionFormat)compressionInt;
            }
            else if (Enum.TryParse<WebGLCompressionFormat>(compressionRaw, true, out var formatEnum))
            {
                PlayerSettings.WebGL.compressionFormat = formatEnum;
            }
            else
            {
                Debug.LogError($"[Builder] ❌ Invalid compressionFormat: '{compressionRaw}' is not a valid WebGLCompressionFormat.");
                EditorApplication.Exit(1);
                return;
            }
        }

        // CLI: -strictShaderVariantMatching true|false
        var strictShaderVariantMatching = ParseBool(GetArg("strictShaderVariantMatching"));
        if (strictShaderVariantMatching.HasValue)
        {
            PlayerSettings.strictShaderVariantMatching = strictShaderVariantMatching.Value;
        }

        // CLI: -optCodegen size|speed → IL2CPP code generation optimization
        string optCodegen = GetArg("optCodegen");
        if (!string.IsNullOrEmpty(optCodegen))
        {
            if (optCodegen.Equals("size", StringComparison.OrdinalIgnoreCase))
            {
                PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
            }
            else if (optCodegen.Equals("speed", StringComparison.OrdinalIgnoreCase))
            {
                PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSpeed);
            }
        }

        // CLI: -il2cppCompilerConfiguration Release|Debug|Master
        string il2cppCompilerConfiguration = GetArg("il2cppCompilerConfiguration");
        if (!string.IsNullOrEmpty(il2cppCompilerConfiguration) &&
            Enum.TryParse(il2cppCompilerConfiguration, true, out Il2CppCompilerConfiguration config))
        {
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, config);
        }

        // CLI: -strippingLevel Disabled|Low|Medium|High|Minimal
        string strippingRaw = GetArg("managedStrippingLevel");
        if (!string.IsNullOrEmpty(strippingRaw) &&
            Enum.TryParse(strippingRaw, true, out ManagedStrippingLevel level))
        {
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, level);
        }

        var stripMesh = ParseBool(GetArg("stripUnusedMeshComponents"));
        if (stripMesh.HasValue)
        {
            PlayerSettings.stripUnusedMeshComponents = stripMesh.Value;
        }

        var bakeCollisionMeshes = ParseBool(GetArg("bakeCollisionMeshes"));
        if (bakeCollisionMeshes.HasValue)
        {
            PlayerSettings.bakeCollisionMeshes = bakeCollisionMeshes.Value;
        }

        // CLI: -webGLExceptionSupport None|ExplicitlyThrownExceptionsOnly|FullWithoutStacktrace|FullWithStacktrace
        string webGLExceptionSupport = GetArg("webGLExceptionSupport");
        if (!string.IsNullOrEmpty(webGLExceptionSupport) &&
            Enum.TryParse(webGLExceptionSupport, true, out WebGLExceptionSupport support))
        {
            PlayerSettings.WebGL.exceptionSupport = support;
        }

        // CLI: -gcIncremental true|false
        var gcIncremental = ParseBool(GetArg("gcIncremental"));
        if (gcIncremental.HasValue)
        {
            PlayerSettings.gcIncremental = gcIncremental.Value;
        }

        // CLI: -stripEngineCode true|false
        var stripEngineCode = ParseBool(GetArg("stripEngineCode"));
        if (stripEngineCode.HasValue)
        {
            PlayerSettings.stripEngineCode = stripEngineCode.Value;
        }

        // CLI: -development true|false
        var development = ParseBool(GetArg("development"));
        if (development.HasValue)
        {
            EditorUserBuildSettings.development = development.Value;
        }

        // CLI: -connectProfiler true|false
        var connectProfiler = ParseBool(GetArg("connectProfiler"));
        if (connectProfiler.HasValue)
        {
            EditorUserBuildSettings.connectProfiler = connectProfiler.Value;
        }

        // CLI: -allowDebugging true|false
        var allowDebugging = ParseBool(GetArg("allowDebugging"));
        if (allowDebugging.HasValue)
        {
            EditorUserBuildSettings.allowDebugging = allowDebugging.Value;
        }

        // CLI: -buildOptions Development,AllowDebugging,...
        // https://docs.unity3d.com/ScriptReference/BuildOptions.html
        // Get the CLI argument string for buildOptions
        string buildOptionsRaw = GetArg("buildOptions");

        // Check if the string is not null or empty
        if (buildOptionsRaw != null && buildOptionsRaw.Trim().Length > 0)
        {
            // Split the string by commas
            string[] optionStrings = buildOptionsRaw.Split(',');

            // Loop over each option string
            for (int i = 0; i < optionStrings.Length; i++)
            {
                // Trim whitespace from the current option
                string option = optionStrings[i];
                if (option == null) continue;

                string trimmedOption = option.Trim();

                // Check if the trimmed string is not empty
                if (trimmedOption.Length == 0)
                {
                    Debug.LogWarning("[Builder] Skipping empty build option.");
                    continue;
                }

                // Try to parse the string into a BuildOptions enum value (case-insensitive)
                BuildOptions parsedOption;
                bool success = Enum.TryParse<BuildOptions>(trimmedOption, true, out parsedOption);

                // Check if the parsing succeeded and the value is defined in the enum
                if (success && Enum.IsDefined(typeof(BuildOptions), parsedOption))
                {
                    // Combine the parsed flag with the CLI options using bitwise OR
                    _cliBuildOptions = _cliBuildOptions | parsedOption;

                    Debug.Log($"[Builder] ✅ Parsed BuildOption: {parsedOption}");
                }
                else
                {
                    Debug.LogError($"[Builder] ❌ Invalid BuildOption: '{trimmedOption}' is not recognized.");
                    EditorApplication.Exit(1);
                    return;
                }
            }
        }

    }

    public static void BuildWebGL()
    {
        try
        {
            BuildSettingsInspector.PrintAllSettings();
            PrintCurrentSettings("[Builder] 🔎 Current Build Settings (Before)");

            ApplySettingsFromArgs();

            PrintCurrentSettings("[Builder] ✅ Updated Build Settings (After)");
            BuildSettingsInspector.PrintAllSettings();

            BuildOptions options = _cliBuildOptions;

            if (EditorUserBuildSettings.development) options |= BuildOptions.Development;
            if (EditorUserBuildSettings.allowDebugging) options |= BuildOptions.AllowDebugging;
            if (EditorUserBuildSettings.connectProfiler) options |= BuildOptions.ConnectWithProfiler;

            Debug.Log($"[Builder] 🚀 Final BuildOptions = {options}");

            Debug.Log("[Builder] 🚧 Starting WebGL build...");
            var report = BuildPipeline.BuildPlayer(
                GetEnabledScenes(),
                "./Builds/",
                BuildTarget.WebGL,
                options
                );

            var result = report.summary.result;
            Debug.Log($"[Builder] ✅ Build Result: {result}, Total time: {report.summary.totalTime.TotalSeconds:F1}s");

            EditorApplication.Exit(result == BuildResult.Succeeded ? 0 : 1);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Builder] ❌ Exception during build: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
            EditorApplication.Exit(1);
        }
    }
}
