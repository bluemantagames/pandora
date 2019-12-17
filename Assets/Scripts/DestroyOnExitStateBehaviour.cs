using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnExitStateBehaviour : StateMachineBehaviour
{
    void OnStateExit(Animator animator)
    {
        animator.enabled = false;
        Destroy (animator.gameObject);    
    }
}
