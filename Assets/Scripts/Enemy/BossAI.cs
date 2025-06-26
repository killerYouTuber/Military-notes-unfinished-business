using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : EnemyAI
{
    [Header("Boss Abilities")]
    public float aoeCooldown = 15f;
    public float aoeRadius = 5f;
    public int aoeDamage = 20;

    private float lastAoeTime;

    protected override void Update()
    {
        base.Update(); // Базовая логика EnemyAI

        // Проверяем возможность использования AoE-атаки
        if (Time.time - lastAoeTime >= aoeCooldown)
        {
            AoeAttack();
            lastAoeTime = Time.time;
        }
    }

    private void AoeAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage(aoeDamage);
            }
        }

        // Визуализация радиуса атаки в редакторе
        Debug.DrawRay(transform.position, Vector3.up * 3f, Color.red, 1f);
    }
}