using UnityEngine;
using UnityEngine.UI;
using Pandora.Deck.UI;
using Cysharp.Threading.Tasks;
using Pandora.Network;
using Pandora;
using System.Net;

public class DeckSlotBehaviour : MonoBehaviour
{
    public long DeckSlotId;
    public int DeckSlotIndex;
    public bool Active = false;
    public Sprite ActiveImage;
    Sprite oldImage;

    public void ChangeDeckSlot()
    {
        var deckSpotParentBehaviour = gameObject.GetComponentInParent<DeckSlotParentBehaviour>();

        if (deckSpotParentBehaviour)
        {
            _ = deckSpotParentBehaviour.ExecuteChangeDeckSlot(DeckSlotId);
        }
    }

    public void Activate()
    {
        var image = gameObject.GetComponent<Image>();

        if (image != null)
        {
            oldImage = image.sprite;
            image.sprite = ActiveImage;
        }

        Active = true;
    }

    public void Deactivate()
    {
        var image = gameObject.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = oldImage;
        }

        Active = false;
    }
}
