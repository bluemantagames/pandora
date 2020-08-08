using UnityEngine;
using UnityEngine.EventSystems;


namespace Pandora.UI.Modal {
    public class ModalBehaviour : MonoBehaviour, IPointerClickHandler
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

        public void AddContent(GameObject content) {
            Instantiate(content, transform.parent);
        }
    }

}