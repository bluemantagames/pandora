using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Localization;
using Cysharp.Threading.Tasks;


namespace Pandora.UI.HUD
{
    public class MatchEndStendardsBehaviour : MonoBehaviour
    {
        public LocalizedString WinText, LossText;
        public GameObject WinTextObject, LossTextObject;
        public VideoClip WinClip, LossClip;
        public bool IsDisabled = true;
        

        public async UniTaskVoid PlayStendard(bool isWin) {
            if (IsDisabled) return;

            VideoClip clip;

            var player = GetComponent<VideoPlayer>();

            if (isWin) {
                clip = WinClip;

                WinTextObject.GetComponent<Text>().text = await WinText.GetLocalizedString();
            } else {
                clip = LossClip;

                LossTextObject.GetComponent<Text>().text = await LossText.GetLocalizedString();
            }

            player.clip = clip;

            player.Play();
        }
    }
}