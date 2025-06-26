using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0f;

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    public float topClamp = -90f;
    public float bottomClamp = 90f;

    [Header("Плавное смещение камеры при приседании")]
    public float standingCamY = 1.6f;
    public float crouchingCamY = 1.0f;
    public float camTransitionSpeed = 8f;
    private float targetCamY;

    void Start()
    {
        // Блокируем курсор, только если мы не в сцене-заставке
        if (SceneManager.GetActiveScene().name != "briv1")
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        // Устанавливаем начальную высоту камеры
        targetCamY = standingCamY;
        SetCameraYInstant(targetCamY);
    }

    void Update()
    {
        // Плавно двигаем камеру к нужной высоте
        Vector3 localPos = cam.transform.localPosition;
        localPos.y = Mathf.Lerp(localPos.y, targetCamY, Time.deltaTime * camTransitionSpeed);
        cam.transform.localPosition = localPos;
    }

    public void SetCrouchCamera(bool isCrouch)
    {
        targetCamY = isCrouch ? crouchingCamY : standingCamY;
    }

    private void SetCameraYInstant(float y)
    {
        Vector3 localPos = cam.transform.localPosition;
        localPos.y = y;
        cam.transform.localPosition = localPos;
    }

    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f,0f);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }
}
