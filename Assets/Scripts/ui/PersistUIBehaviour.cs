using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistUIBehaviour : MonoBehaviour
{
    static PersistUIBehaviour instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
