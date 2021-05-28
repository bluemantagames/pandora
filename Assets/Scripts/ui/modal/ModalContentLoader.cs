using Pandora.UI.Modal;
using UnityEngine;
using Pandora;

namespace Pandora.UI.Menu.Modal
{
    public class ModalContentLoader : MonoBehaviour
    {
        public GameObject ModalContent;

        private MenuModalBehaviour modalContainer;

        void Awake()
        {
            var mainCanvas = GameObject.Find(Constants.MAIN_CANVAS_OBJECT_NAME).gameObject;
            modalContainer = mainCanvas.GetComponentInChildren<MenuModalBehaviour>();

            var modalContentCanvas = ModalContent?.GetComponent<Canvas>();
            if (modalContentCanvas != null) modalContentCanvas.enabled = false;
        }

        public void Open()
        {
            if (ModalContent == null) return;

            var modalContentObject = Instantiate(ModalContent, transform.parent, true);
            var modalContentCanvas = modalContentObject.GetComponent<Canvas>();

            modalContainer.AppendComponent(modalContentObject.gameObject, null);

            modalContentObject.GetComponent<ModalInit>()?.Init();

            modalContainer.Show();
        }
    }
}