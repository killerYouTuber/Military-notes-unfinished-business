using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public LayerMask interactionLayer;

    [Header("Ссылки")]
    public Camera playerCamera;
    public Transform GunsOnHands;
    public TMP_Text descriptionText;

    private Interactable currentInteractable;

    void Start()
    {
        if (descriptionText == null)
        {
            GameObject messegsObject = GameObject.Find("Messegs");
            if (messegsObject != null)
            {
                descriptionText = messegsObject.GetComponent<TMP_Text>();
            }
        }

        if (descriptionText != null)
        {
            descriptionText.text = "";
        }
    }

    void Update()
    {
        if (PauseManager.IsPaused) return;
        FindInteractable();
    }

    void FindInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            Interactable newInteractable = hit.collider.GetComponent<Interactable>();
            if (newInteractable != null)
            {
                if (newInteractable != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLookAway();
                    }

                    currentInteractable = newInteractable;
                    currentInteractable.OnLook();

                    DebugUI.ShowPrompt(currentInteractable.interactionPrompt);
                    if (descriptionText != null)
                    {
                        descriptionText.text = currentInteractable.description;
                    }
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.OnLookAway();
            currentInteractable = null;
            DebugUI.HidePrompt();
            if (descriptionText != null)
            {
                descriptionText.text = "";
            }
        }
    }

    public void HandleInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}