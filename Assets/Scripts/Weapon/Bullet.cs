using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float speed = 10f;
    public float damage = 10f;
    public float gravity = 0.1f; // ���� ����������
    public LayerMask destroyOnLayers;
    private Vector3 velocity;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        velocity = transform.forward * speed;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = velocity;
        }
        Destroy(gameObject, 10f);
    }

    void FixedUpdate()
    {
        // ��������� ����������
        velocity += Vector3.down * gravity * Time.fixedDeltaTime;

        if (rb != null)
        {
            rb.velocity = velocity;
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Bullet>() != null)
            return;

        // Проверка на хедшот по тегу
        if (other.CompareTag("Head"))
        {
            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage * 2); // Двойной урон за хедшот
                Debug.Log("Headshot!");
                Destroy(gameObject);
                return;
            }
        }

        // Обычный урон
        EnemyHealth enemyHealthNormal = other.GetComponent<EnemyHealth>();
        if (enemyHealthNormal != null)
        {
            enemyHealthNormal.TakeDamage(damage);
            Debug.Log("Враг получил урон!");
            Destroy(gameObject);
            return;
        }

        // Проверка по LayerMask
        if (((1 << other.gameObject.layer) & destroyOnLayers.value) != 0)
        {
            Debug.Log($"Пуля игрока столкнулась с объектом слоя {LayerMask.LayerToName(other.gameObject.layer)} и будет уничтожена");
            Destroy(gameObject);
            return;
        }
        else
        {
            //  ������ �� ���-�� ������
            Debug.Log($"���������� ������: {other.name}");
        }
    }
}