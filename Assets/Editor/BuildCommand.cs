using UnityEditor;
using System.Linq;
using System;
using System.Diagnostics;

/// <summary>Pretty much from the unity gitlab CI scripts, with some modifications</summary>
static class BuildCommand
{
    static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    static string[] GetEnabledScenes()
    {
        return (
            from scene in EditorBuildSettings.scenes
            where scene.enabled
            where !string.IsNullOrEmpty(scene.path)
            select scene.path
        ).ToArray();
    }

    static BuildTarget GetBuildTarget()
    {
        string buildTargetName = GetArgument("customBuildTarget");
        Console.WriteLine(":: Received customBuildTarget " + buildTargetName);

        if (buildTargetName.ToLower() == "android")
        {
#if !UNITY_5_6_OR_NEWER
			// https://issuetracker.unity3d.com/issues/buildoptions-dot-acceptexternalmodificationstoplayer-causes-unityexception-unknown-project-type-0
			// Fixed in Unity 5.6.0
			// side effect to fix android build system:
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
#endif
        }

        return ToEnum<BuildTarget>(buildTargetName, BuildTarget.NoTarget);
    }

    static string GetBuildPath()
    {
        string buildPath = GetArgument("customBuildPath");
        Console.WriteLine(":: Received customBuildPath " + buildPath);
        if (buildPath == "")
        {
            throw new Exception("customBuildPath argument is missing");
        }
        return buildPath;
    }

    static string GetBuildName()
    {
        string buildName = GetArgument("customBuildName");
        Console.WriteLine(":: Received customBuildName " + buildName);
        if (buildName == "")
        {
            throw new Exception("customBuildName argument is missing");
        }
        return buildName;
    }

    static string GetFixedBuildPath(BuildTarget buildTarget, string buildPath, string buildName)
    {
        if (buildTarget.ToString().ToLower().Contains("windows"))
        {
            buildName = buildName + ".exe";
        }
        return buildPath + buildName;
    }

    static BuildOptions GetBuildOptions()
    {
        string buildOptions = GetArgument("customBuildOptions");
        return buildOptions == "AcceptExternalModificationsToPlayer" ? BuildOptions.AcceptExternalModificationsToPlayer : BuildOptions.None;
    }

    // https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
    static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
        {
            return defaultValue;
        }

        return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
    }

    static string getEnv(string key, bool secret = false, bool verbose = true)
    {
        var env_var = Environment.GetEnvironmentVariable(key);
        if (verbose)
        {
            if (env_var != null)
            {
                if (secret)
                {
                    Console.WriteLine(":: env['" + key + "'] set");
                }
                else
                {
                    Console.WriteLine(":: env['" + key + "'] set to '" + env_var + "'");
                }
            }
            else
            {
                Console.WriteLine(":: env['" + key + "'] is null");
            }
        }
        return env_var;
    }

    [MenuItem("Custom Build/Android build")]
    static void PerformBuild()
    {
        Console.WriteLine(":: Performing build");

        var path = EditorUtility.SaveFolderPanel("Choose Location of apk", "", "androidbuild");
        var p = new Process();

        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = "/bin/bash";
        p.StartInfo.Arguments = "-c 'git describe --tags'";

        p.Start();

        var output = p.StandardOutput.ReadToEnd();

        p.WaitForExit();

        PlayerSettings.bundleVersion = output.Trim();

        BuildPipeline.BuildPlayer(GetEnabledScenes(), path + "/pandora.apk", BuildTarget.Android, GetBuildOptions());

        Console.WriteLine(":: Done with build");
    }
}
