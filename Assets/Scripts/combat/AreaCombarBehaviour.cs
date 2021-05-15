using System;
using UnityEngine;
using Pandora;
using Pandora.Combat;
using Pandora.Engine;

public class AreaCombarBehaviour : MonoBehaviour, CombatBehaviour
{
    public int Damage = 10;
    bool isBackswinging = false;
    Enemy target = null;
    uint timeSinceLastProjectile = 0;
    public bool isAttacking { get; private set; } = false;
    public bool IsDisabled { get; set; } = false;
    public int AggroRangeCells = 3;
    public int AttackRangeEngineUnits = 2000;
    public string animationStateName;
    public int attackCooldownMs = 600, backswingMs = 200;
    public CombatType combatType
    {
        get
        {
            return CombatType.Ranged;
        }
    }
    public GameObject AttackVFX;

    public void StopAttacking()
    {
        isAttacking = false;
        target = null;

        var animator = GetComponent<Animator>();

        animator.SetBool("Attacking", false);
    }

    /** Returns true if enemy has died*/
    public void AttackEnemy(Enemy target, uint timeLapse)
    {
        if (IsDisabled) return;

        var animator = GetComponent<Animator>();

        var cappedBackswingMs = Math.Max(1, backswingMs);
        var cappedAttackCooldownMs = Math.Max(1, attackCooldownMs);

        if (!isAttacking)
        {
            this.target = target;

            animator.SetBool("Attacking", true);
            animator.speed = 0;

            isAttacking = true;
        }

        var direction = ((Vector2)target.enemy.transform.position - (Vector2)transform.position).normalized;

        animator.SetFloat("BlendX", direction.x);
        animator.SetFloat("BlendY", direction.y);

        animator.Play(animationStateName, 0, timeSinceLastProjectile / (float)(cappedAttackCooldownMs + cappedBackswingMs));

        timeSinceLastProjectile += timeLapse;

        if (timeSinceLastProjectile >= cappedAttackCooldownMs && !isBackswinging)
        {
            SpawnVFX();

            isBackswinging = true;
        }
        else if (timeSinceLastProjectile >= cappedAttackCooldownMs + cappedBackswingMs)
        {
            timeSinceLastProjectile = 0;

            isBackswinging = false;
        }
    }

    public void ProjectileCollided(Enemy target)
    {
        var lifeComponent = target?.enemy.GetComponent<LifeComponent>();
        lifeComponent?.AssignDamage(Damage, new BaseAttack(gameObject));
    }

    public void OnDead() { }

    public bool IsInAggroRange(Enemy enemy)
    {
        var engineComponent = GetComponent<EngineComponent>();
        var engine = engineComponent.Engine;

        return engine.IsInHitboxRangeCells(engineComponent.Entity, enemy.enemyEntity, AggroRangeCells);
    }

    public bool IsInAggroRange(GridCell cell)
    {
        var engineComponent = GetComponent<EngineComponent>();
        var engine = engineComponent.Engine;

        return Vector2.Distance(cell.vector, engineComponent.Entity.GetCurrentCell().vector) < AggroRangeCells;
    }

    public bool IsInAttackRange(Enemy enemy)
    {
        var engineComponent = GetComponent<EngineComponent>();
        var engine = engineComponent.Engine;

        return engine.IsInHitboxRange(engineComponent.Entity, enemy.enemyEntity, AttackRangeEngineUnits);
    }

    public void SpawnVFX()
    {

    }
}
