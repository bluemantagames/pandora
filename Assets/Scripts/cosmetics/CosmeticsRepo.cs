using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pandora.Cosmetics {
    public class CosmeticsRepo {
        bool nameTagsLoaded = false;
        Dictionary<string, GameObject> nameTags;
        string nameTagsLabel = "NameTag";

        public UniTask ApplyNameTagCosmetic(string nameTagKey, GameObject nameTag) =>
            ApplyCosmetic(nameTagKey, nameTag);

        public async UniTask ApplyCosmetic(string cosmeticKey, GameObject obj) {
            var asset = await Addressables.LoadAssetAsync<GameObject>(cosmeticKey);
            var applier = asset.GetComponent<CosmeticApplier>();

            applier.Apply(obj);
        }
        
    }
}