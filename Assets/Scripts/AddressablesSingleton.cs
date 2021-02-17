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

        public UniTask DownloadDependencies()
        {
            return Addressables.DownloadDependenciesAsync(unitsAssetsLabel).ToUniTask();
        }

        public UniTask LoadUnits()
        {
            return Addressables
                .LoadAssetsAsync<GameObject>(unitsAssetsLabel, loadedUnit =>
                {
                    var unitName = loadedUnit.name;

                    Logger.Debug($"Loaded addressable unit: {unitName}");

                    units.Add(unitName, loadedUnit);
                })
                .ToUniTask();
        }
    }
}