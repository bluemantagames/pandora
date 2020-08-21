using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace Pandora.Engine {
    public class HitboxLoader {
        private static HitboxLoader _instance = null;

        public static HitboxLoader Instance {
            get {
                if (_instance == null) {
                    _instance = new HitboxLoader();
                }

                return _instance;
            }
        }

        public SerializableDictionary<string, EngineHitbox> Hitboxes;

        public HitboxLoader() {
            var jsonTextFile = Resources.Load<TextAsset>("hitboxes");

            Hitboxes = JsonUtility.FromJson<SerializableDictionary<string, EngineHitbox>>(jsonTextFile.text);

            Logger.Debug($"Loaded {Hitboxes.Count} hitboxes");
        }

        public void Save() {
            var filePath = $"{Application.dataPath}/Scripts/engine/Resources/hitboxes.txt";

            var json = JsonUtility.ToJson(Hitboxes);

            Debug.Log($"Saving {json} / {Hitboxes.Count} to {filePath}");

            File.WriteAllText(filePath, json);
        }
    }
}