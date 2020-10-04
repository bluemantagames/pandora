using UnityEngine;
using UnityEngine.UI;

namespace Pandora.Resource.Mana {
    public class ManaMaskComponent: MonoBehaviour {
        public float OriginalWidth;
        Image image;

        void Start() {
            image = GetComponent<Image>();
            OriginalWidth = image.rectTransform.rect.width;

            SetChildProperSize();
        }

        void SetPercent(float percent) {
        }

        /// <summary>
        /// This needs a proper explaination:
        ///
        /// We need the mana bar to be as big as the mask image, but it cannot be anchored to the four corners
        /// (stretched) because we also need to be able to reduce the size of this mask independently of the
        /// size and position of the child. For this reason, here we rescale the child to fit the mask
        /// again
        /// </summary>
        void SetChildProperSize() {
            var manaImage = transform.GetChild(0);
            var childRectTransform = manaImage.GetComponent<Image>().rectTransform;

            childRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, image.rectTransform.rect.width);
            childRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, image.rectTransform.rect.height);
            childRectTransform.position = image.rectTransform.position;
        }

        void Update() {
        }
    }
}