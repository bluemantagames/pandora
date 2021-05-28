using System;
using System.Collections.Generic;
using Pandora.Network.Data.Users;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Pandora
{
    public class AddressablesSingleton
    {
        public Dictionary<string, GameObject> units = new Dictionary<string, GameObject> { };
        public Dictionary<string, GameObject> NameTags = new Dictionary<string, GameObject> { };
        private static AddressablesSingleton privateInstance = null;
        string remoteAssetsLabel = "Remote", unitAssetsLabel = "Unit";
        float loadingProgress = 0f;

        private AddressablesSingleton() { }

        public static AddressablesSingleton instance
        {
            get
            {
                if (privateInstance == null)
                    privateInstance = new AddressablesSingleton();

                return privateInstance;
            }
        }

        /// <summary>
        /// Return the remote addressable size to download.
        /// If zero the bundle is cached in the device.
        /// </summary>
        /// <returns>A Task with a long describing the bundle size</returns>
        public UniTask<long> GetAddressablesSize()
        {
            return Addressables
                .GetDownloadSizeAsync(remoteAssetsLabel)
                .ToUniTask();
        }

        /// <summary>
        /// Clear the cache on the device.
        /// </summary>
        public UniTask ClearDependenciesCache()
        {
            return Addressables.ClearDependencyCacheAsync(remoteAssetsLabel, false).ToUniTask();
        }

        /// <summary>
        /// Download the dependencies from the remote server.
        /// </summary>
        /// <param name="progressHandler">A function called with the updated download progress.</param>
        public UniTask DownloadDependencies(Action<float> progressHandler = null)
        {
            var progressManager = Progress.Create<float>(progressHandler);
            return Addressables.DownloadDependenciesAsync(remoteAssetsLabel).ToUniTask(progress: progressManager);
        }

        /// <summary>
        /// Load all the necessary addressables.
        /// </summary>
        /// <param name="progressHandler">A function called with the updated progress (this is not actually working).</param>
        public UniTask LoadAddressables(Action<float> progressHandler = null)
        {
            var progressManager = Progress.Create<float>(progressHandler);

            return Addressables
                .LoadAssetsAsync<GameObject>(remoteAssetsLabel, loadedAddressable =>
                {
                    var addressableName = loadedAddressable.name;

                    Logger.Debug($"Loaded addressable: {addressableName}");

                    if (loadedAddressable.tag == "NameTag")
                        NameTags.Add(loadedAddressable.name, loadedAddressable);
                    else if (!units.ContainsKey(addressableName))
                        units.Add(addressableName, loadedAddressable);
                })
                .ToUniTask(progress: progressManager);
        }
    }
}