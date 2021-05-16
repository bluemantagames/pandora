using System;
using UnityEngine;
using Pandora;
using Pandora.Combat;
using Pandora.Engine;

public class AreaCombarBehaviour : MonoBehaviour, CombatBehaviour
{
    bool isBackswinging = false;
    Enemy target = null;
    uint timeSinceLastProjectile = 0;
    float shotDirectionThreshold = 0.5f;
    int vfxTicksElapsed = 0;
    uint vfxTotalTimeElapsed = 0;
    Vector2Int enemyDirection;
    CombatVFXFixer combatVFXFixer;
    ParticleSystem[] particles = new ParticleSystem[0];
    public float VfxAnimationStartTime = 0f, VfxEndAnimationTime = 0f;
    public int VfxAnimationDelay = 0;
    public int Damage = 10;
    public bool isAttacking { get; private set; } = false;
    public bool IsDisabled { get; set; } = false;
    public int AggroRangeCells = 3;
    public int AttackRangeEngineUnits = 2000;
    public string AnimationStateName;
    public int AttackCooldownMs = 3000, BackswingMs = 200;
    public GameObject CombatVFX;
    public CombatType combatType
    {
        get
        {
            return CombatType.Ranged;
        }
    }

    public void Awake()
    {
        if (CombatVFX != null)
        {
            combatVFXFixer = CombatVFX.GetComponent<CombatVFXFixer>();
            particles = CombatVFX.GetComponentsInChildren<ParticleSystem>();
        }
    }

    public void StopAttacking()
    {
        isAttacking = false;
        target = null;

        var animator = GetComponent<Animator>();

        animator.SetBool("Attacking", false);
    }

    public void AttackEnemy(Enemy target, uint timeLapse)
    {
        if (IsDisabled) return;

        var animator = GetComponent<Animator>();

        var cappedBackswingMs = Math.Max(1, BackswingMs);
        var cappedAttackCooldownMs = Math.Max(1, AttackCooldownMs);

        if (!isAttacking)
        {
            this.target = target;

            animator.SetBool("Attacking", true);
            animator.speed = 0;

            isAttacking = true;
        }

        var direction = ((Vector2)target.enemy.transform.position - (Vector2)transform.position).normalized;
        var computedDirection = GetShotDirection(direction);

        if (combatVFXFixer != null && CombatVFX != null)
            CombatVFX.transform.localRotation = combatVFXFixer.FixedShotRotation(computedDirection);

        animator.SetFloat("BlendX", direction.x);
        animator.SetFloat("BlendY", direction.y);

        animator.Play(AnimationStateName, 0, timeSinceLastProjectile / (float)(cappedAttackCooldownMs + cappedBackswingMs));

        timeSinceLastProjectile += timeLapse;

        if (timeSinceLastProjectile >= cappedAttackCooldownMs && !isBackswinging)
        {
            vfxTicksElapsed = 0;
            vfxTotalTimeElapsed = 0;

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

    public void PlayVFXNextFrame(uint timeLapsed)
    {
        if (CombatVFX == null) return;

        vfxTicksElapsed++;
        vfxTotalTimeElapsed += timeLapsed;

        float animationPercent = (float)vfxTotalTimeElapsed / AttackCooldownMs;

        var time =
            vfxTotalTimeElapsed > VfxAnimationDelay
                ? VfxAnimationStartTime + (animationPercent * (VfxEndAnimationTime - VfxAnimationStartTime))
                : 0;


        foreach (var particle in particles)
        {
            particle.Pause();

            particle.Simulate(time);

            Debug.Log($"Particle time {particle.time}, animationPercent {animationPercent}");
        }
    }

    Vector2Int GetShotDirection(Vector2 enemyDirection)
    {
        var direction = new Vector2Int();

        if (enemyDirection != null)
        {
            direction.x = enemyDirection.x > shotDirectionThreshold ? 1 : enemyDirection.x < -shotDirectionThreshold ? -1 : 0;
            direction.y = enemyDirection.y > shotDirectionThreshold ? 1 : enemyDirection.y < -shotDirectionThreshold ? -1 : 0;
        }

        return direction;
    }
}
