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
        public string WinFileName = "win.webm", LossFileName = "loss.webm";
        public LocalizedString WinText, LossText;
        public GameObject WinTextObject, LossTextObject;
        

        public async UniTaskVoid PlayStendard(bool isWin) {
            var player = GetComponent<VideoPlayer>();

            string filename;

            if (isWin) {
                filename = WinFileName;

                WinTextObject.GetComponent<Text>().text = await WinText.GetLocalizedString();
            } else {
                filename = LossFileName;

                LossTextObject.GetComponent<Text>().text = await LossText.GetLocalizedString();
            }

            var url = $"file://{Application.streamingAssetsPath}/{filename}";

            #if UNITY_ANDROID && !UNITY_EDITOR
            url = $"jar:file://{Application.dataPath}!/assets/{filename}";
            #endif

            player.url = url;

            player.Play();
        }
    }
}