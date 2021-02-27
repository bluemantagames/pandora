using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.UI.Menu;
using Pandora;

public class NavbarButtonBehaviour : MonoBehaviour
{
    public MenuView TargetView;
    public ViewsContainerBehaviour ViewsContainer;
    public bool Active;
    public bool Disabled = false;
    GameObject panelComponent;
    Image panelImageComponent;
    Color panelImageColor;

    void Awake()
    {
        if (Active) Activate();

        panelComponent = transform.GetChild(0).gameObject;
        panelImageComponent = panelComponent.GetComponent<Image>();
        panelImageColor = panelImageComponent.color;
    }

    void Start()
    {
        // Make it opaque if disabled
        var opaqueColor = new Color(panelImageColor.r, panelImageColor.g, panelImageColor.b, 0.1f);
        panelImageComponent.color = (Disabled) ? opaqueColor : panelImageColor;
    }

    public void HandlePress()
    {
        if (ViewsContainer == null || Disabled) return;

        AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MENU_VIEW_CHANGE, new Dictionary<string, object>() {
            {"from", ViewsContainer.CurrentView.name},
            {"to", TargetView.ToString()}
        });

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
        var others = parent.transform.parent.GetComponentsInChildren<NavbarButtonBehaviour>();

        foreach (var otherButton in others)
        {
            if (otherButton == this) continue;
            otherButton.Deactivate();
        }
    }
}
