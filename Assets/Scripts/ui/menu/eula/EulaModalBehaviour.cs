using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EulaModalBehaviour : MonoBehaviour
{
    Canvas eulaCanvas;
    string eulaKey = "isEulaAccepted";
    int eulaAcceptedValue = 1;
    int eulaNonAcceptedValue = 0;

    private bool IsAlreadyAccepted()
    {
        var eulaValue = PlayerPrefs.GetInt(eulaKey, eulaNonAcceptedValue);
        return eulaValue == eulaAcceptedValue;
    }

    private void Show()
    {
        eulaCanvas.enabled = true;
    }

    private void Hide()
    {
        eulaCanvas.enabled = false;
    }

    public void AcceptEula()
    {
        PlayerPrefs.SetInt(eulaKey, eulaAcceptedValue);
        Hide();
    }

    void Awake()
    {
        var isEulaAccepted = IsAlreadyAccepted();

        eulaCanvas = GetComponent<Canvas>();

        if (!isEulaAccepted) Show();
        else Hide();
    }
}