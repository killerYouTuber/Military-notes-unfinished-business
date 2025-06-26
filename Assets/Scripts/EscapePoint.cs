using UnityEngine;

public class EscapePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && MissionManager.Instance != null)
        {
            MissionManager.Instance.OnEscapePointReached();
        }
    }
} 