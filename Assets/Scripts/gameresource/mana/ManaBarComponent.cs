using UnityEngine;
using UnityEngine.UI;

namespace Pandora.Resource.Mana
{
    public class ManaBarComponent : MonoBehaviour
    {
        bool _reserved = false;
        GameObject EmptyManaBarComponent;

        public ManaMaskComponent MaskComponent;
        public Sprite EmptyManaImage;
        public Sprite BlockedManaImage;

        public bool Reserved
        {
            set
            {
                _reserved = value;

                if (_reserved)
                    SetReserved();
                else
                    SetEmpty();
            }

            get => _reserved;
        }

        void Start()
        {
            MaskComponent = GetComponentInChildren<ManaMaskComponent>();
            EmptyManaBarComponent = transform.GetChild(0).gameObject;
            SetEmpty();
        }

        void SetReserved()
        {
            if (EmptyManaBarComponent == null) return;

            EmptyManaBarComponent.GetComponent<Image>().sprite = BlockedManaImage;
        }

        void SetEmpty()
        {
            if (EmptyManaBarComponent == null) return;

            EmptyManaBarComponent.GetComponent<Image>().sprite = EmptyManaImage;
        }
    }
}