using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using Cysharp.Threading.Tasks;

namespace Pandora.Resource.Mana {
    public class ManaMaskComponent: MonoBehaviour {
        public float OriginalWidth = 0f;
        Image image;
        AnimationCurve earnCurve = null;
        bool childSet = false;
        RectTransform childRectTransform;

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
            childRectTransform = transform.GetChild(0).GetComponent<Image>().rectTransform;

            // Set the resolution now, before playing with the various objects sizes
            Screen.fullScreen = false;
            Screen.SetResolution(720, 1280, false);

            Application.targetFrameRate = 30;
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
        /// We need the mana bar to be as big as the mask image, but it cannot be anchored permanently to the four corners
        /// (stretched) because we also need to be able to reduce the size of this mask independently of the
        /// size and position of the child. For this reason, here we deanchor the mana image from the mask and reset the position because of
        /// Unity layouting quirks
        /// </summary>
        void setChildProperSize() {
            var horizontalSize = childRectTransform.rect.width;
            var verticalSize = childRectTransform.rect.height;

            childRectTransform.anchorMin = new Vector2(0, 0);
            childRectTransform.anchorMax = new Vector2(0, 0);

            childRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, horizontalSize);
            childRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, verticalSize);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);

            childRectTransform.position = transform.parent.position;
        }

        void Update() {
            // It appears that the widths are actually zero for a few frames
            // hence the various if conditions
            if (OriginalWidth == 0f && image.rectTransform.rect.width != 0) {
                OriginalWidth = image.rectTransform.rect.width;
            }

            if (OriginalWidth != 0 && childRectTransform.rect.width != 0 && !childSet) {
                setChildProperSize();

                childSet = true;
            }

            if (IsPlaying && Percent < 1f)
                Percent = earnCurve.Evaluate(Time.time);
        }
    }
}