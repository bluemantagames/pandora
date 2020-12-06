using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Pandora.UI.Layout
{
    public class TopLeftLayoutSelfController : UIBehaviour, ILayoutSelfController
    {
        //Fields in the inspector used to manipulate the RectTransform
        public Vector3 position;
        public Vector3 m_Rotation;
        public Vector2 m_Scale;

        //This handles horizontal aspects of the layout (derived from ILayoutController)
        public virtual void SetLayoutHorizontal()
        {
            //Move and Rotate the RectTransform appropriately
            UpdateRectTransform();
        }

        //This handles vertical aspects of the layout
        public virtual void SetLayoutVertical()
        {
            //Move and Rotate the RectTransform appropriately
            UpdateRectTransform();
        }

        //This tells when there is a change in the inspector
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            UpdateRectTransform();
        }

#endif

        //This tells when there has been a change to the RectTransform's settings in the inspector
        protected override void OnRectTransformDimensionsChange()
        {
            //Update the RectTransform position, rotation and scale
            UpdateRectTransform();
        }

        void UpdateRectTransform()
        {
            //Fetch the RectTransform from the GameObject
            RectTransform rectTransform = GetComponent<RectTransform>();
            var parentRectTransform = transform.parent.GetComponent<RectTransform>();

            var upperLeft = parentRectTransform.TransformPoint(new Vector2(parentRectTransform.rect.xMin, parentRectTransform.rect.yMax));

            //Change the position and rotation of the RectTransform
            rectTransform.SetPositionAndRotation(upperLeft, Quaternion.identity);
        }
    }
}