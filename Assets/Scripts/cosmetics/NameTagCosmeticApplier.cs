using UnityEngine;
using UnityEngine.UI;

namespace Pandora.Cosmetics {
    public class NameTagCosmeticApplier : MonoBehaviour, CosmeticApplier
    {
        public Sprite NameTag;

        public void Apply(GameObject obj)
        {
            obj.GetComponent<Image>().sprite = NameTag;
        }
    }
}