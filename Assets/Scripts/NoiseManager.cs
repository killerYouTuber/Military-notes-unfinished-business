using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Статический менеджер для обработки звуковых событий в игре.
/// Враги (слушатели) подписываются на события, а источники шума (например, игрок) их генерируют.
/// </summary>
public static class NoiseManager
{
    // Список всех активных слушателей (врагов) на сцене.
    private static List<EnemyAI> listeners = new List<EnemyAI>();

    /// <summary>
    /// Регистрирует слушателя в системе.
    /// </summary>
    public static void RegisterListener(EnemyAI listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    /// <summary>
    /// Удаляет слушателя из системы.
    /// </summary>
    public static void UnregisterListener(EnemyAI listener)
    {
        listeners.Remove(listener);
    }

    /// <summary>
    /// Генерирует звуковое событие (шум) в указанной точке с заданным радиусом.
    /// Оповещает всех слушателей в радиусе действия.
    /// </summary>
    /// <param name="position">Центр шума.</param>
    /// <param name="radius">Радиус шума.</param>
    public static void EmitNoise(Vector3 position, float radius)
    {
        // Создаем копию списка, чтобы избежать проблем при изменении коллекции во время итерации
        // (например, если враг будет уничтожен после реакции на шум).
        foreach (var listener in new List<EnemyAI>(listeners))
        {
            if (listener != null && listener.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(listener.transform.position, position);
                if (distance <= radius)
                {
                    // У слушателя есть прямая видимость до источника шума?
                    // (Эта проверка теперь будет внутри самого врага)
                    listener.OnNoiseHeard(position);
                }
            }
        }
    }
} 