using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок прошел через чекпоинт!");
            
            GameManager.Instance.SaveCheckpoint();
            isActivated = true;
            
            // Опционально: можно добавить визуальный/звуковой эффект,
            // чтобы игрок понял, что чекпоинт сработал.
            // Например, поменять материал или выключить объект.
            GetComponent<Renderer>().material.color = Color.green; // Пример
        }
    }
} 