using UnityEngine;
using UnityEngine.UI;
using Pandora;
using Pandora.Network;
using Pandora.Deck.UI;
using Cysharp.Threading.Tasks;
using System.Net;

public class DeckSlotParentBehaviour : MonoBehaviour
{
    public GameObject DeckSpotsParent;
    public Button DeckSlotButton;
    ModelSingleton modelSingleton = ModelSingleton.instance;
    ApiControllerSingleton apiControllerSingleton = ApiControllerSingleton.instance;

    void Awake()
    {
        if (modelSingleton.DeckSlots == null) return;
        if (DeckSlotButton == null) return;
        if (DeckSpotsParent == null) return;

        var deckSpotParentBehaviour = DeckSpotsParent.GetComponent<DeckSpotParentBehaviour>();
        var activeDeckSlot = modelSingleton.User.activeDeckSlot;

        if (deckSpotParentBehaviour == null) return;

        for (var index = 0; index < modelSingleton.DeckSlots.Count; index++)
        {
            var deckSlot = modelSingleton.DeckSlots[index];
            var deckSlotButton = Instantiate(DeckSlotButton, DeckSlotButton.transform.position, DeckSlotButton.transform.rotation);
            deckSlotButton.transform.SetParent(gameObject.transform);
            var buttonRect = deckSlotButton.GetComponent<RectTransform>();

            // Change value
            deckSlotButton.GetComponentInChildren<Text>().text = $"{index}";

            // Change the deck slot id
            var deckSlotBehaviour = deckSlotButton.GetComponent<DeckSlotBehaviour>();

            if (deckSlotBehaviour != null)
            {
                deckSlotBehaviour.DeckSlotId = deckSlot.id;
                deckSlotBehaviour.DeckSlotIndex = index;

                // Activate and load the
                // relative deck if necessary
                if (deckSlot.id == activeDeckSlot)
                {
                    deckSlotBehaviour.Activate();
                    deckSpotParentBehaviour.LoadSavedDeck(index);
                }
            }
        }

        // Remove first button
        Destroy(DeckSlotButton.gameObject);
    }

    public async UniTaskVoid ExecuteChangeDeckSlot(long deckSlotId, int deckSlotIndex)
    {
        if (DeckSpotsParent == null) return;

        var deckSpotParentBehaviour = DeckSpotsParent.GetComponent<DeckSpotParentBehaviour>();

        if (deckSpotParentBehaviour == null) return;

        var token = modelSingleton.Token;

        if (token == null) return;

        var response = await apiControllerSingleton.ActiveDeckSlotUpdate(deckSlotId, token);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            deckSpotParentBehaviour.Reset();
            deckSpotParentBehaviour.LoadSavedDeck(deckSlotIndex);

            // Update the model
            modelSingleton.User.activeDeckSlot = deckSlotId;

            UpdateActiveSlot();
        }
    }

    public void UpdateActiveSlot()
    {
        var activeDeckSlot = modelSingleton.User.activeDeckSlot;

        foreach (Transform child in transform)
        {
            var deckSlot = child.GetComponent<DeckSlotBehaviour>();

            if (deckSlot == null) continue;

            if (deckSlot.Active && deckSlot.DeckSlotId != activeDeckSlot)
                deckSlot.Deactivate();

            if (!deckSlot.Active && deckSlot.DeckSlotId == activeDeckSlot)
                deckSlot.Activate();
        }
    }
}
