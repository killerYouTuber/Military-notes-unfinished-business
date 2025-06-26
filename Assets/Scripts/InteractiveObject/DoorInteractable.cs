using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : Interactable
{
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    [Header("Настройки двери")]
    public float rotationAngle = 90f;
    public float rotationSpeed = 2f;
    public bool clockwise = true;
    
    [Tooltip("Если активно, дверь всегда закрыта и не открывается при взаимодействии")]
    public bool alwaysClosed = false;

    public bool IsOpen => isOpen;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, clockwise ? rotationAngle : -rotationAngle, 0);

        // Восстанавливаем состояние двери из sceneDoorStateList
        if (GameManager.Instance != null && GameManager.CurrentSaveData != null)
        {
            var saved = GameManager.CurrentSaveData.sceneDoorStateList.Find(d => d.uniqueID == uniqueID);
            if (saved != null)
            {
                if (saved.isOpen)
                {
                    transform.rotation = openRotation;
                    isOpen = true;
                }
                else
                {
                    transform.rotation = closedRotation;
                    isOpen = false;
                }
            }
        }
    }

    public override void Interact()
    {
        if (alwaysClosed)
        {
            Debug.Log($"{name}: Дверь всегда закрыта (alwaysClosed)");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(RotateDoor());

        // Сохраняем состояние двери после взаимодействия через sceneDoorStateList
        if (GameManager.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            var saveData = GameManager.CurrentSaveData;
            if (saveData != null)
            {
                var doorSave = saveData.sceneDoorStateList.Find(d => d.uniqueID == uniqueID);
                if (doorSave != null)
                {
                    doorSave.isOpen = !isOpen; // isOpen уже изменится в RotateDoor
                    doorSave.position = transform.position;
                    doorSave.rotation = transform.rotation;
                }
                else
                {
                    saveData.sceneDoorStateList.Add(new SceneDoorStateSave {
                        uniqueID = uniqueID,
                        isOpen = !isOpen,
                        position = transform.position,
                        rotation = transform.rotation
                    });
                }
            }
            // Убираем автоматическое сохранение игры при взаимодействии с дверью
            // GameManager.Instance.SaveGame(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator RotateDoor()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;
        isOpen = !isOpen;

        float time = 0;
        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, time);
            time += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
