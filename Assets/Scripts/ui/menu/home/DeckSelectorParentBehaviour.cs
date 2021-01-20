using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora;

public class DeckSelectorParentBehaviour : MonoBehaviour
{
    public GameObject DeckSelectorButton;
    private PlayerModelSingleton playerModel;

    void Awake()
    {
        playerModel = PlayerModelSingleton.instance;

        Setup();
    }

    private void Setup()
    {
        if (DeckSelectorButton == null) return;
        if (playerModel?.DeckSlots == null) return;

        var deckSlots = playerModel.DeckSlots;

        for (var index = 0; index < deckSlots.Count; index++)
        {
            var deckSlot = deckSlots[index];
            var deckButton = Instantiate(DeckSelectorButton);
            deckButton.GetComponentInChildren<Text>().text = $"{index}";
            deckButton.transform.SetParent(gameObject.transform);
        }

        Destroy(DeckSelectorButton);
    }
}
