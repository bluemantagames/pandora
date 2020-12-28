using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSelectorButtonBehaviour : MonoBehaviour
{
    public long? DeckId;
    public bool Active;
    public Color ActiveColor = new Color(196, 94, 110);
    Color oldColor;

    public void HandleClick()
    {
        if (DeckId == null) return;

        var deckId = (long)DeckId;


    }

    public void Activate()
    {
        var image = gameObject.GetComponent<Image>();

        if (image != null)
        {
            oldColor = image.color;
            image.color = ActiveColor;
        }

        Active = true;
    }

    public void Deactivate()
    {
        var image = gameObject.GetComponent<Image>();

        if (image != null)
        {
            image.color = oldColor;
        }

        Active = false;
    }
}
