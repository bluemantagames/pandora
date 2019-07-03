using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombatBehaviour : MonoBehaviour
{
    float damage = 10f;
    LifeComponent target = null;

    public void MeleeAssignDamage() {
        if (target == null)
            return;

        target.AssignDamage(damage);
    }
}
