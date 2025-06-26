using UnityEngine;
using System.Linq;

public class MiniAI : EnemyAI
{
    [Header("Heal Rune Settings")]
    public GameObject healRunePrefab;
    public float healRuneCooldown = 30f;
    public float healRuneRadius = 5f;
    public int healAmount = 10;
    public float allyCheckRadius = 10f;

    private float lastRuneTime;
    private LayerMask allyMask;

    protected override void Start()
    {
        base.Start();
        allyMask = LayerMask.GetMask("Enemy");
    }

    protected override void Update()
    {
        base.Update(); // �������� ������ �������������/����� �� EnemyAI

        if (Time.time - lastRuneTime >= healRuneCooldown)
        {
            TrySpawnHealRune();
            lastRuneTime = Time.time;
        }
    }

    private void TrySpawnHealRune()
    {
        if (healRunePrefab == null) return;

        Collider[] allies = Physics.OverlapSphere(transform.position, allyCheckRadius, allyMask);
        Vector3 spawnPosition = allies.Length > 0
            ? FindBestHealPosition(allies)
            : transform.position;

        SpawnRuneAtPosition(spawnPosition);
    }

    private Vector3 FindBestHealPosition(Collider[] allies)
    {
        var groupedPositions = allies
            .Select(a => new Vector3(
                Mathf.Round(a.transform.position.x / 2f) * 2f,
                a.transform.position.y,
                Mathf.Round(a.transform.position.z / 2f) * 2f
            ))
            .GroupBy(pos => pos)
            .OrderByDescending(group => group.Count())
            .FirstOrDefault();

        return groupedPositions?.Key ?? transform.position;
    }

    private void SpawnRuneAtPosition(Vector3 position)
    {
        Instantiate(healRunePrefab, position, Quaternion.identity);
        HealAlliesInRadius(position);
        Debug.Log($"���� ������� � �������: {position}");
    }

    private void HealAlliesInRadius(Vector3 position)
    {
        Collider[] allies = Physics.OverlapSphere(position, healRuneRadius, allyMask);
        foreach (var ally in allies)
        {
            if (ally.TryGetComponent<Health>(out var health))
            {
                health.Heal(healAmount);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ������������ ������� ������ ���������
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, allyCheckRadius);

        // ������������ ������� �������
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRuneRadius);
    }
}