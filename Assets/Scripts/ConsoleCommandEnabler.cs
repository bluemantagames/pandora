using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;

public class ConsoleCommandEnabler : MonoBehaviour
{
    public KeyCode EventKeyCode = KeyCode.BackQuote;
    public GameObject Console;

    bool isEnabled = false;

    float? lastTime = null;
    float debouceTime = 0.2f;

    void Start() {
        QuantumMacros.DefineMacro("spawn-enemy", "spawn-enemy-unit-params \"HalfOrc\" 14,7");
    }

    void OnGUI()
    {
        if (Event.current.alt && Event.current.keyCode == EventKeyCode) {
            if (lastTime != null && Time.time - lastTime.Value < debouceTime) return;

            lastTime = Time.time;

            isEnabled = !isEnabled;

            Debug.Log($"Console enabled: {isEnabled}");

            Console.SetActive(isEnabled);
        }
    }
}
