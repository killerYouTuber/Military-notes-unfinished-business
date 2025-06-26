using UnityEngine;

public class WallRepeater : MonoBehaviour
{
    [Header("Настройки повторения стены")]
    [Tooltip("Префаб секции стены (например, UNIConcrete_fence_v1_s)")]
    public GameObject wallPrefab;
    [Tooltip("Сколько секций создать")]
    public int count = 5;
    [Tooltip("Смещение между секциями (обычно по X или Z)")]
    public Vector3 offset = new Vector3(3, 0, 0);

    void Start()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("WallRepeater: Не назначен префаб стены!");
            return;
        }
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + offset * i;
            Instantiate(wallPrefab, pos, transform.rotation, transform);
        }
    }
} 