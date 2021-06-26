using UnityEngine;
using System;
using System.Collections.Generic;
using Pandora.Network;
using Pandora.Cosmetics;
using Cysharp.Threading.Tasks;
using Pandora.UI.Menu.Modal;


namespace Pandora.UI.Menu.NameTag
{
    public class NameTagModalBehaviour : MonoBehaviour, ModalInit
    {
        public GameObject RowPrefab, NameTagContainerPrefab, Viewport;
        public String DefaultNameTagName = "DefaultNameTag";

        String nameTagKind = "NameTag";

        Dictionary<string, NameTagContainerBehaviour> nameTagContainers = new Dictionary<string, NameTagContainerBehaviour> { };
        HashSet<string> unlockedNameTags;
        GameObject selectedNameTag = null;


        public void Init()
        {
            unlockedNameTags = new HashSet<string> { DefaultNameTagName };

            GetComponent<Canvas>().enabled = false;

            AsyncStart().Forget();
        }


        async UniTaskVoid AsyncStart()
        {
            var nameTags = CosmeticsRepo.Instance.LoadNameTags();
            var nameTagNames = new List<string>(nameTags.Keys);

            GameObject row = null;

            for (var i = 0; i < nameTagNames.Count; i++)
            {
                if (i % 2 == 0)
                {
                    row = Instantiate(RowPrefab, Vector2.zero, Quaternion.identity, Viewport.transform);
                }

                var nameTagName = nameTagNames[i];
                var nameTagContainer = Instantiate(NameTagContainerPrefab, Vector2.zero, Quaternion.identity, row.transform);
                var nameTagContainerBehaviour = nameTagContainer.GetComponent<NameTagContainerBehaviour>();

                nameTagContainers[nameTagName] = nameTagContainerBehaviour;

                nameTagContainerBehaviour.SetupNameTag(nameTags[nameTagName], this);

                if (nameTagName != DefaultNameTagName)
                    nameTagContainerBehaviour.MarkLocked();

                nameTagContainerBehaviour.MarkUnselected();

                Debug.Log($"Marked {nameTagName} as locked + unselected");
            }


            var cosmetics = await ApiControllerSingleton.instance.GetUnlockedCosmetics(nameTagKind, PlayerModelSingleton.instance.Token);

            foreach (var nameTag in cosmetics.Body.result)
            {
                unlockedNameTags.Add(nameTag.name);

                Debug.Log($"Unlocking {nameTag.name}");

                var nameTagBehaviour = nameTagContainers[nameTag.name];

                nameTagBehaviour.MarkUnlocked();

                if (nameTag.selectedAt != "")
                {
                    UISelect(nameTag.name);
                }
            }

            if (selectedNameTag == null)
            {
                UISelect(DefaultNameTagName);
            }

            GetComponent<Canvas>().enabled = true;
        }

        public void Select(string name)
        {
            AsyncSelect(name).Forget();
        }

        public async UniTaskVoid AsyncSelect(string name)
        {
            if (name != DefaultNameTagName)
                await ApiControllerSingleton.instance.SelectCosmetic(name, PlayerModelSingleton.instance.Token);
            else
                await ApiControllerSingleton.instance.UnselectCosmetic(nameTagKind, PlayerModelSingleton.instance.Token);

            UISelect(name);
        }

        public void UISelect(string name)
        {
            if (!unlockedNameTags.Contains(name)) return;

            selectedNameTag?.GetComponent<NameTagContainerBehaviour>()?.MarkUnselected();

            var nameTagBehaviour = nameTagContainers[name];

            Debug.Log($"Selecting {name}");

            selectedNameTag?.GetComponent<NameTagContainerBehaviour>().MarkUnselected();

            nameTagBehaviour.MarkSelected();

            selectedNameTag = nameTagBehaviour.gameObject;
        }
    }
}