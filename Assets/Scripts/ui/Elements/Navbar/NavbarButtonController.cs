using System;
using UnityEngine;
using UnityEngine.UIElements;

public class NavbarButtonController
{
    private Button buttonElement;
    private Action onPressFn;
    public bool IsActive { get; private set; } = false;

    public NavbarButtonController(Button buttonElement)
    {
        this.buttonElement = buttonElement;
        onPressFn = null;

        Setup();
    }

    public NavbarButtonController(Button buttonElement, Action onPressFn)
    {
        this.buttonElement = buttonElement;
        this.onPressFn = onPressFn;

        Setup();
    }

    public void Activate()
    {
        IsActive = true;
        buttonElement.style.backgroundColor = new StyleColor(Color.red);
    }

    public void Deactivate()
    {
        IsActive = false;
        buttonElement.style.backgroundColor = new StyleColor(StyleKeyword.None);
    }

    private void Setup()
    {
        this.buttonElement.RegisterCallback<ClickEvent>(_ => HandleClick());
    }

    private void HandleClick()
    {
        Logger.Debug("Clicked a button!");

        if (!IsActive)
        {
            Activate();
            if (onPressFn != null) onPressFn();
        }
    }
}