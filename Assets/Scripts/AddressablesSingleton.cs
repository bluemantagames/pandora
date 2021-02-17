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
        string unitsAssetsLabel = "Unit";
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

        public UniTask ClearDependenciesCache()
        {
            return Addressables.ClearDependencyCacheAsync(unitsAssetsLabel, false).ToUniTask();
        }

        public UniTask DownloadDependencies(Action<float> progressHandler = null)
        {
            var progressManager = Progress.Create<float>(progressHandler);
            return Addressables.DownloadDependenciesAsync(unitsAssetsLabel).ToUniTask(progress: progressManager);
        }

        public UniTask LoadUnits(Action<float> progressHandler = null)
        {
            var progressManager = Progress.Create<float>(progressHandler);

            return Addressables
                .LoadAssetsAsync<GameObject>(unitsAssetsLabel, loadedUnit =>
                {
                    var unitName = loadedUnit.name;

                    Logger.Debug($"Loaded addressable unit: {unitName}");

                    units.Add(unitName, loadedUnit);
                })
                .ToUniTask(progress: progressManager);
        }
    }
}