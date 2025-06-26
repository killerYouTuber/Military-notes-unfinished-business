using UnityEngine;
using System; // Добавляем для использования Guid

public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Уникальный идентификатор для этого объекта. Должен быть уникальным среди всех интерактивных объектов в одной сцене.")]
    public string uniqueID;

    void Awake()
    {
        // Если uniqueID пустой, генерируем новый GUID
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = Guid.NewGuid().ToString();
            Debug.Log($"[Interactable] Сгенерирован новый uniqueID для {gameObject.name}: {uniqueID}");
        }
        else
        {
            Debug.Log($"[Interactable] Используется существующий uniqueID для {gameObject.name}: {uniqueID}");
        }
    }

    public enum InteractionType { Door, Weapon, Hole, Documents, Medkit, BoxAmmo }

    [Header("Настройки взаимодействия")]
    public InteractionType interactionType;
    public string interactionPrompt = "Нажмите E";
    [Tooltip("Описание объекта, которое будет отображаться при наведении в отдельном UI элементе.")]
    public string description;

    public abstract void Interact();

    // Новые виртуальные методы для реакции на наведение взгляда
    public virtual void OnLook() { }
    public virtual void OnLookAway() { }

    // Виртуальный метод для применения сохраненного состояния
    // Дочерние классы должны переопределить его для своей специфической логики
    public virtual void ApplySavedState(string stateData)
    {
        // Базовая реализация ничего не делает, дочерние классы переопределят
    }
}