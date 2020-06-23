using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pandora;

public class BeckSlotParentBehaviour : MonoBehaviour
{
    public Button DeckSlotButton;
    private ModelSingleton modelSingleton = ModelSingleton.instance;

    void Awake()
    {
        if (modelSingleton.DeckSlots == null) return;
        if (DeckSlotButton == null) return;

        for (var index = 0; index < modelSingleton.DeckSlots.Count; index++)
        {
            var deckSlot = modelSingleton.DeckSlots[index];
            var deckSlotButton = Instantiate(DeckSlotButton, DeckSlotButton.transform.position, DeckSlotButton.transform.rotation);
            deckSlotButton.transform.SetParent(gameObject.transform);
            var buttonRect = deckSlotButton.GetComponent<RectTransform>();

            // Change position
            var buttonPosition = buttonRect.position;
            buttonPosition.x += 75 * index;
            buttonRect.position = buttonPosition;

            // Change value
            deckSlotButton.GetComponentInChildren<Text>().text = $"{index}";

            // Change the deck slot id
            deckSlotButton.GetComponent<DeckSlotBehaviour>().DeckSlotId = deckSlot.id;
        }

        // Remove first button
        Destroy(DeckSlotButton);
    }
}
