using UnityEngine;

namespace Pandora
{
    public class ProjectileHideFixer : MonoBehaviour
    {
        public int ShowAfterFrames = 0;
        SpriteRenderer spriteRenderer;
        int passedFrames = 0;
        Color spriteColor;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteColor = spriteRenderer.color;

            spriteRenderer.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, passedFrames < ShowAfterFrames ? 0f : 1f);
        }

        void Update()
        {
            spriteRenderer.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, passedFrames < ShowAfterFrames ? 0f : 1f);
            if (passedFrames < ShowAfterFrames) passedFrames++;
        }
    }
}