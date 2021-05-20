using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pandora.Cosmetics
{
    public class CosmeticsRepo
    {
        private static CosmeticsRepo _repo = null;

        public static CosmeticsRepo Instance
        {
            get
            {
                if (_repo == null)
                {
                    _repo = new CosmeticsRepo();
                }

                return _repo;
            }
        }

        bool nameTagsLoaded = false;
        Dictionary<string, GameObject> nameTags;
        string nameTagsLabel = "NameTag";

        public UniTask ApplyNameTagCosmetic(string nameTagKey, GameObject nameTag) =>
            ApplyCosmetic(nameTagKey, nameTag, "DefaultNameTag");

        public async UniTask ApplyCosmetic(string cosmeticKey, GameObject obj, string defaultKey)
        {
            GameObject asset;

            try {
                asset = await Addressables.LoadAssetAsync<GameObject>(cosmeticKey);
            } catch (Exception e) {
                Logger.DebugWarning($"Error while loading cosmetic {cosmeticKey} {e.Message}, loading default {defaultKey}");

                asset = await Addressables.LoadAssetAsync<GameObject>(defaultKey);
            }

            var applier = asset.GetComponent<CosmeticApplier>();

            applier.Apply(obj);
        }

    }
}