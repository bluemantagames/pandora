using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.UI.Menu.NameTag
{
    public class NameTagContainerBehaviour : MonoBehaviour
    {
        public float UnselectedAlpha = 0.29f;
        GameObject lockedIcon = null;
        public GameObject LockIcon;

        public void MarkSelected() {
            var color  = GetComponentInChildren<Image>().color;

            color.a = 1f;
        }

        public void MarkUnselected() {
            var color  = GetComponentInChildren<Image>().color;

            color.a = UnselectedAlpha;
        }


        public void MarkLocked() {
            var child = GetComponentInChildren<Image>().gameObject;

            Instantiate(LockIcon, Vector2.zero, Quaternion.identity);
        }
    }
}