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

        public void ApplyNameTagCosmetic(string nameTagKey, GameObject nameTag) =>
            ApplyCosmetic(nameTagKey, nameTag, "DefaultNameTag");

        public Dictionary<string, GameObject> LoadNameTags() {
            return AddressablesSingleton.instance.NameTags;
        }

        void DoNothing(GameObject obj) {
            Debug.Log($"Loaded cosmetic {obj.name}");
        }

        public void ApplyCosmetic(string cosmeticKey, GameObject obj, string defaultKey)
        {
            GameObject asset;

            var tags = LoadNameTags();

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