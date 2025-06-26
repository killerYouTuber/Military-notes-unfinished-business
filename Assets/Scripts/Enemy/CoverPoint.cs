using UnityEngine;

public enum CoverType { Low, High }

public class CoverPoint : MonoBehaviour
{
    public CoverType coverType = CoverType.Low;
    public Vector3 coverDirection = Vector3.forward; // В какую сторону защищает укрытие
    [HideInInspector] public bool isOccupied = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = coverType == CoverType.Low ? Color.green : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + coverDirection);
    }
}
 