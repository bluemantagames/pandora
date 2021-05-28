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
        Dictionary<string, GameObject> nameTags = new Dictionary<string, GameObject> {};
        string nameTagsLabel = "NameTag";

        public UniTask ApplyNameTagCosmetic(string nameTagKey, GameObject nameTag) =>
            ApplyCosmetic(nameTagKey, nameTag, "DefaultNameTag");

        public async UniTask<Dictionary<string, GameObject>> LoadNameTags() {
            if (nameTags.Count == 0) {
                var tags = await Addressables.LoadAssetsAsync<GameObject>(nameTagsLabel, DoNothing);

                foreach (var tag in tags) {
                    nameTags[tag.name] = tag;
                }
            }

            return nameTags;
        }

        void DoNothing(GameObject obj) {}

        public async UniTask ApplyCosmetic(string cosmeticKey, GameObject obj, string defaultKey)
        {
            GameObject asset;

            var tags = await LoadNameTags();

            try {
                asset = tags[cosmeticKey];
            } catch (Exception e) {
                Logger.DebugWarning($"Error while loading cosmetic {cosmeticKey} {e.Message}, loading default {defaultKey}");

                asset = tags[defaultKey];
            }

            var applier = asset.GetComponent<CosmeticApplier>();

            applier.Apply(obj);
        }

    }
}