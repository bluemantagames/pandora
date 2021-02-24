using System;
using System.Collections.Generic;
using System.Linq;
using UnityBuilderAction.Input;
using UnityBuilderAction.Reporting;
using UnityBuilderAction.Versioning;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace UnityBuilderAction
{
    static class Builder
    {
        public static string StagingProfile = "Staging";
        public static string ProductionProfile = "Production";

        public static void BuildProject()
        {
            // Gather values from args
            var options = ArgumentsParser.GetValidatedOptions();

            // Manage the correct profile
            var version = options["buildVersion"];
            var splittedVersion = version.Split('.');
            var minorVersion = Int32.Parse(splittedVersion.Last());
            var profileName = minorVersion % 2 == 0 ? StagingProfile : ProductionProfile;

            // Setting the correct profile
            var profileSettings = AddressableAssetSettingsDefaultObject.Settings.profileSettings;
            var profileId = profileSettings.GetProfileId(profileName);
            AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;

            // Execute the build
            AddressableAssetSettings.BuildPlayerContent();

            // Gather values from project
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();

            // Get all buildOptions from options
            BuildOptions buildOptions = BuildOptions.None;
            foreach (string buildOptionString in Enum.GetNames(typeof(BuildOptions)))
            {
                if (options.ContainsKey(buildOptionString))
                {
                    BuildOptions buildOptionEnum = (BuildOptions)Enum.Parse(typeof(BuildOptions), buildOptionString);
                    buildOptions |= buildOptionEnum;
                }
            }

            // Define BuildPlayer Options
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = options["customBuildPath"],
                target = (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]),
                options = buildOptions
            };

            // Set version for this build
            VersionApplicator.SetVersion(options["buildVersion"]);
            VersionApplicator.SetAndroidVersionCode(options["androidVersionCode"]);

            // Apply Android settings
            if (buildPlayerOptions.target == BuildTarget.Android)
                AndroidSettings.Apply(options);

            // Perform build
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

            // Summary
            BuildSummary summary = buildReport.summary;
            StdOutReporter.ReportSummary(summary);

            // Result
            BuildResult result = summary.result;
            StdOutReporter.ExitWithResult(result);
        }
    }
}
