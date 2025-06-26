using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("[PlayerManager] Обнаружен дубликат PlayerManager. Уничтожаю.");
            Destroy(gameObject);
        }
    }
    #endregion

    public GameObject player;

    // Метод для динамического присвоения игрока
    public void SetPlayer(GameObject playerObject)
    {
        if (playerObject == null)
        {
            Debug.LogError("[PlayerManager] Попытка установить null в качестве игрока!");
            return;
        }

        player = playerObject;
        Debug.Log($"[PlayerManager] Игрок установлен: {playerObject.name} в позиции {playerObject.transform.position}");
    }

    // Метод для проверки наличия игрока
    public bool HasPlayer()
    {
        return player != null;
    }

    // Метод для получения текущего игрока
    public GameObject GetPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("[PlayerManager] Попытка получить игрока, когда он не установлен!");
        }
        return player;
    }
}