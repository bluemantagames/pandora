using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Cosmetics;

namespace Pandora.UI.Menu.NameTag
{
    public class NameTagContainerBehaviour : MonoBehaviour
    {
        public float UnselectedAlpha = 0.29f;
        GameObject lockedIcon = null;
        public GameObject LockIcon, NameTagImage;
        GameObject lockIconChild = null;
        NameTagModalBehaviour modalBehaviour;
        GameObject nameTagCosmetic;

        public void MarkSelected() {
            var color  = NameTagImage.GetComponent<Image>().color;

            color.a = 1f;

            NameTagImage.GetComponent<Image>().color = color;
        }

        public void MarkUnselected() {
            var color  = NameTagImage.GetComponent<Image>().color;

            color.a = UnselectedAlpha;

            NameTagImage.GetComponent<Image>().color = color;
        }


        public void MarkLocked() {
            lockIconChild = Instantiate(LockIcon, Vector2.zero, Quaternion.identity, NameTagImage.transform);

            lockIconChild.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
        }

        public void MarkUnlocked() {
            if (lockIconChild != null) Destroy(lockIconChild);
        }

        public void SetupNameTag(GameObject cosmetic, NameTagModalBehaviour nameTagModalBehaviour) {
            modalBehaviour = nameTagModalBehaviour;
            nameTagCosmetic = cosmetic;

            var nameTagApplier = cosmetic.GetComponent<NameTagCosmeticApplier>();

            NameTagImage.GetComponent<Image>().sprite = nameTagApplier.NameTag;
        }

        public void SelectNameTag() {
            modalBehaviour?.Select(nameTagCosmetic.name);
        }

    }
}