using UnityEngine;

namespace Pandora
{
    public class ProjectileHideFixer : MonoBehaviour
    {
        public int ShowAfterFrames = int.MaxValue;
        SpriteRenderer spriteRenderer;
        int passedFrames = 0;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1f, 1f, 1f, passedFrames < ShowAfterFrames ? 0f : 1f);
        }

        void Update()
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, passedFrames < ShowAfterFrames ? 0f : 1f);
            if (passedFrames < ShowAfterFrames) passedFrames++;
        }
    }
}