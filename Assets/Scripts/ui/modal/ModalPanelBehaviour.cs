using UnityEngine;
using UnityEngine.EventSystems;


namespace Pandora.UI.Modal {
    public class ModalPanelBehaviour : MonoBehaviour, IPointerClickHandler
    {
        GameObject modalCanvas;

        void Start() {
            modalCanvas = transform.parent.gameObject;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Logger.Debug("Destroying modal");

            Destroy(modalCanvas);
        }
    }

}