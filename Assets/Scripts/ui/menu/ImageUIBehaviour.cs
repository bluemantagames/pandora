using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.UI
{
    public class ImageUIBehaviour : MonoBehaviour
    {
        void Awake()
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            var rawImageComponent = GetComponent<RawImage>();
            var renderTexture = (RenderTexture)rawImageComponent.texture;

            renderTexture.Release();
            renderTexture.width = screenWidth;
            renderTexture.height = screenHeight;
            renderTexture.Create();
        }
    }
}