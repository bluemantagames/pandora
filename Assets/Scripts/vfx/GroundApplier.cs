using UnityEngine;

namespace Pandora.VFX
{
    public class GroundApplier : MonoBehaviour, VFXApplier
    {

        /// <summary>Applies the VFX to the target gameobject</summary>
        public GameObject Apply(GameObject target)
        {
            var targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
            var groundAnchor = target.GetComponentInChildren<UnitGroundAnchor>();
            var bounds = targetSpriteRenderer.bounds;

            GameObject vfx;

            if (groundAnchor != null)
            {
                var vfxPosition = new Vector3(0f, 0f, 0f);
                vfx = Instantiate(gameObject, vfxPosition, gameObject.transform.rotation, groundAnchor.transform);

                var vfxRect = vfx.GetComponent<RectTransform>();

                if (vfxRect != null) vfxRect.anchoredPosition = vfxPosition;
            }
            else
            {
                var vfxPosition = new Vector3(bounds.min.x + bounds.extents.x, bounds.min.y + bounds.extents.y, 0f);

                vfx = Instantiate(gameObject, vfxPosition, gameObject.transform.rotation, target.transform);
            }

            return vfx;
        }

    }
}