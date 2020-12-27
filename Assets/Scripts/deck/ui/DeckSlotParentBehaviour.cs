using UnityEngine;
using UnityEngine.UI;
using Pandora;
using Pandora.Network;
using Pandora.Deck.UI;
using Cysharp.Threading.Tasks;
using System.Net;
using Pandora.UI.Menu;
using Pandora.UI.Menu.Event;
using Pandora.Events;

public class DeckSlotParentBehaviour : MonoBehaviour
{
    public GameObject DeckSpotsParent;
    public Button DeckSlotButton;
    PlayerModelSingleton playerModelSingleton;
    ApiControllerSingleton apiControllerSingleton;
    MenuEventsSingleton menuEventsSingleton;
    DeckSpotParentBehaviour deckSpotParentBehaviour;

    void Awake()
    {
        playerModelSingleton = PlayerModelSingleton.instance;
        apiControllerSingleton = ApiControllerSingleton.instance;
        menuEventsSingleton = MenuEventsSingleton.instance;
        deckSpotParentBehaviour = DeckSpotsParent.GetComponent<DeckSpotParentBehaviour>();

        Setup();

        menuEventsSingleton.EventBus.Subscribe<ViewActive>(new EventSubscriber<MenuEvent>(ViewActiveHandler, "ViewActiveHandler"));
    }

    void Setup()
    {
        if (playerModelSingleton.DeckSlots == null) return;
        if (DeckSlotButton == null) return;
        if (DeckSpotsParent == null) return;
        if (deckSpotParentBehaviour == null) return;

        var activeDeckSlot = playerModelSingleton.User.activeDeckSlot;

        for (var index = 0; index < playerModelSingleton.DeckSlots.Count; index++)
        {
            var deckSlot = playerModelSingleton.DeckSlots[index];
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
                }
            }
        }

        // Load deck
        deckSpotParentBehaviour.LoadSavedDeck(activeDeckSlot);

        // Remove first button
        Destroy(DeckSlotButton.gameObject);
    }

    void ViewActiveHandler(MenuEvent ev)
    {
        var viewActive = ev as ViewActive;
        var activeDeckSlot = playerModelSingleton?.User?.activeDeckSlot;

        if (viewActive.ActiveView != MenuView.DeckView) return;
        if (activeDeckSlot == null) return;
        if (deckSpotParentBehaviour == null) return;

        // This method must be used because
        // when we are switching a view, it
        // will be enabled, and all the grid layout
        // inside could have `0` since they are not
        // instantly calculated.
        LayoutRebuilder.ForceRebuildLayoutImmediate(deckSpotParentBehaviour.gameObject.GetComponent<RectTransform>());

        deckSpotParentBehaviour.LoadSavedDeck((long)activeDeckSlot);
    }

    public async UniTaskVoid ExecuteChangeDeckSlot(long deckSlotId)
    {
        if (DeckSpotsParent == null) return;

        var deckSpotParentBehaviour = DeckSpotsParent.GetComponent<DeckSpotParentBehaviour>();

        if (deckSpotParentBehaviour == null) return;

        var token = playerModelSingleton.Token;

        if (token == null) return;

        var response = await apiControllerSingleton.ActiveDeckSlotUpdate(deckSlotId, token);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            deckSpotParentBehaviour.Reset();
            deckSpotParentBehaviour.LoadSavedDeck(deckSlotId);

            // Update the model
            playerModelSingleton.User.activeDeckSlot = deckSlotId;

            UpdateActiveSlot();
        }
    }

    public void UpdateActiveSlot()
    {
        var activeDeckSlot = playerModelSingleton.User.activeDeckSlot;

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
