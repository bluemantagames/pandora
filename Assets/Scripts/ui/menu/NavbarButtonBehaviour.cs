using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.UI.Menu;

public class NavbarButtonBehaviour : MonoBehaviour
{
    public MenuView TargetView;
    public ViewsContainerBehaviour ViewsContainer;
    public bool Active;

    void Awake()
    {
        if (Active) Activate();
    }

    public void HandlePress()
    {
        if (ViewsContainer == null) return;

        ViewsContainer.ShowView(TargetView);

        DeactivateOthers();
        Activate();
    }

    private void Activate()
    {
        Active = true;
        GetComponent<Image>().color = Color.grey;
    }

    private void Deactivate()
    {
        Active = false;

        var deactivatedColor = Color.grey;
        deactivatedColor.a = 0;

        GetComponent<Image>().color = deactivatedColor;
    }

    private void DeactivateOthers()
    {
        var parent = gameObject.transform.parent;
        var others = parent.GetComponentsInChildren<NavbarButtonBehaviour>();

        foreach (var otherButton in others)
        {
            if (otherButton == this) continue;
            otherButton.Deactivate();
        }
    }
}
