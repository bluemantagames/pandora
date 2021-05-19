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
        /// Load all the addressables units.
        /// </summary>
        /// <param name="progressHandler">A function called with the updated progress (this is not actually working).</param>
        public UniTask LoadUnits(Action<float> progressHandler = null)
        {
            var progressManager = Progress.Create<float>(progressHandler);

            return Addressables
                .LoadAssetsAsync<GameObject>(unitAssetsLabel, loadedUnit =>
                {
                    var unitName = loadedUnit.name;

                    Logger.Debug($"Loaded addressable unit: {unitName}");

                    if (!units.ContainsKey(unitName))
                        units.Add(unitName, loadedUnit);
                })
                .ToUniTask(progress: progressManager);
        }
    }
}