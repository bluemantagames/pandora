using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Pandora.Editor
{
    public class AddressablesBuilder
    {
        public static string ProfileFlag = "--profile";
        public static string DefaultProfileName = "Staging";


        static void BuildAddressables()
        {
            // Manage args
            var args = Environment.GetCommandLineArgs();
            var profileFlagIndex = Array.IndexOf(args, ProfileFlag);
            var profileName = profileFlagIndex > -1 && args.Count() >= profileFlagIndex + 1 ? args[profileFlagIndex + 1] : DefaultProfileName;

            // Setting the correct profile
            var profileSettings = AddressableAssetSettingsDefaultObject.Settings.profileSettings;
            var profileId = profileSettings.GetProfileId(profileName);
            AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;

            // Execute the build
            AddressableAssetSettings.BuildPlayerContent();
        }
    }
}