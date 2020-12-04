using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using Cysharp.Threading.Tasks;

namespace Pandora.Resource.Mana {
    public class ManaMaskComponent: MonoBehaviour {
        public float OriginalWidth;
        Image image;
        AnimationCurve earnCurve = null;
        bool childSet = false;

        float _percent = 0f;

        public bool IsPlaying {
            get => earnCurve != null;
        }

        public float Percent {
            set {
                _percent = value;

                var currentWidth = _percent * OriginalWidth;

                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
            }

            get => _percent;
        }

        void Start() {
            image = GetComponent<Image>();
            OriginalWidth = image.rectTransform.rect.width;
            //setChildProperSize();
        }

        public void PlayEarnAnimation() {
            var timeEnd = (1f - Percent) * LocalManaBehaviourScript.RoundingTimelapse;

            earnCurve = AnimationCurve.EaseInOut(Time.time, Percent, Time.time + timeEnd, 1f);
        }

        public void Reset() {
            Percent = 0;
            earnCurve = null;
        }

        public void StopEarnAnimation() {
            earnCurve = null;
        }

        public void SetPercent(float percent) {
            var currentWidth = percent * OriginalWidth;

            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
        }

        /// <summary>
        /// This needs a proper explaination:
        ///
        /// We need the mana bar to be as big as the mask image, but it cannot be anchored to the four corners
        /// (stretched) because we also need to be able to reduce the size of this mask independently of the
        /// size and position of the child. For this reason, here we rescale the child to fit the mask
        /// again
        /// </summary>
        void setChildProperSize() {
            var manaImage = transform.GetChild(0);
            var childRectTransform = manaImage.GetComponent<Image>().rectTransform;
            
            childRectTransform.anchorMax = new Vector2(0, 0);
            childRectTransform.anchorMin = new Vector2(0, 0);

            var oldSizeDelta = childRectTransform.sizeDelta;

            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);

            childRectTransform.sizeDelta = oldSizeDelta;
        }

        void Update() {
            if (!childSet) {
                setChildProperSize();

                childSet = true;
            }

            if (IsPlaying && Percent < 1f)
                Percent = earnCurve.Evaluate(Time.time);
        }
    }
}