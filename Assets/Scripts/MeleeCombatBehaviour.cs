using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombatBehaviour : MonoBehaviour
{
    public float attackAnimationMs = 300f;
    public float backswingDurationMs = 100f;
    public Animator animator;
    float timeSinceLastStateChange = 0f;
    LifeComponent target = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
            return;
        
    }
}
