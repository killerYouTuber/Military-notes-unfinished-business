using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputPlayer inputPlayer;
    private GameMotor motor;
    private PlayerShooting shooting;
    private PlayerInteraction interaction;
    private GameLook gameLook;
    private GrenadeSystem grenadeSystem;

    private void Awake()
    {
        inputPlayer = new InputPlayer();
        motor = GetComponent<GameMotor>();
        shooting = GetComponent<PlayerShooting>();
        interaction = GetComponent<PlayerInteraction>();
        gameLook = GetComponent<GameLook>();
        grenadeSystem = GetComponent<GrenadeSystem>();
    }

    private void OnEnable()
    {
        PauseManager.OnResumeGame += HandleResumeGame;
        PauseManager.OnPauseGame += HandlePauseGame;
        inputPlayer.Enable();
        
        // --- Карта действий "Walk" ---
        inputPlayer.Walk.Jump.performed += OnJump;
        inputPlayer.Walk.Crouch.performed += OnCrouch;
        inputPlayer.Walk.Sprint.performed += OnSprintStart;
        inputPlayer.Walk.Sprint.canceled += OnSprintEnd;
        inputPlayer.Walk.Interact.performed += OnInteract;
        
        // --- Карта действий "Combat" ---
        inputPlayer.Combat.Fire.started += OnFireStart;
        inputPlayer.Combat.Fire.performed += OnFirePerformed;
        inputPlayer.Combat.Fire.canceled += OnFireEnd;
        inputPlayer.Combat.Aim.performed += OnAimStart;
        inputPlayer.Combat.Aim.canceled += OnAimEnd;
        inputPlayer.Combat.Reload.performed += OnReload;
        inputPlayer.Combat.FireMode.performed += OnFireModeSwitch;
        inputPlayer.Combat.Melee.performed += OnMelee;
        inputPlayer.Combat.Grenade.performed += OnGrenadeToggle;
    }

    private void OnDisable()
    {
        PauseManager.OnResumeGame -= HandleResumeGame;
        PauseManager.OnPauseGame -= HandlePauseGame;
        inputPlayer.Disable();
        
        // Отписываемся от всех событий
        inputPlayer.Walk.Jump.performed -= OnJump;
        inputPlayer.Walk.Crouch.performed -= OnCrouch;
        inputPlayer.Walk.Sprint.performed -= OnSprintStart;
        inputPlayer.Walk.Sprint.canceled -= OnSprintEnd;
        inputPlayer.Walk.Interact.performed -= OnInteract;
        
        inputPlayer.Combat.Fire.started -= OnFireStart;
        inputPlayer.Combat.Fire.performed -= OnFirePerformed;
        inputPlayer.Combat.Fire.canceled -= OnFireEnd;
        inputPlayer.Combat.Aim.performed -= OnAimStart;
        inputPlayer.Combat.Aim.canceled -= OnAimEnd;
        inputPlayer.Combat.Reload.performed -= OnReload;
        inputPlayer.Combat.FireMode.performed -= OnFireModeSwitch;
        inputPlayer.Combat.Melee.performed -= OnMelee;
        inputPlayer.Combat.Grenade.performed -= OnGrenadeToggle;
    }

    private void FixedUpdate()
    {
        if (PauseManager.IsPaused)
        {
            motor.ProcessMove(Vector2.zero);
            return;
        }

        // Временный лог для диагностики ввода движения
        Debug.Log($"[Input] Movemoment: {inputPlayer.Walk.Movemoment.ReadValue<Vector2>()}");

        motor.ProcessMove(inputPlayer.Walk.Movemoment.ReadValue<Vector2>()); 
        gameLook.ProcessLook(inputPlayer.Walk.PlayerLook.ReadValue<Vector2>());
    }
    
    // --- Методы-обработчики событий ---
    
    private void OnJump(InputAction.CallbackContext context) => motor.ProcessJump();
    private void OnCrouch(InputAction.CallbackContext context) => motor.ToggleCrouch();
    private void OnSprintStart(InputAction.CallbackContext context) => motor.ProccessSprint();
    private void OnSprintEnd(InputAction.CallbackContext context) => motor.ResetBoost();
    private void OnFireStart(InputAction.CallbackContext context) => shooting.HandleShootingStartInput();
    private void OnFirePerformed(InputAction.CallbackContext context) => shooting.HandleContinuousShootingInput();
    private void OnFireEnd(InputAction.CallbackContext context) => shooting.HandleShootingEndInput();
    private void OnAimStart(InputAction.CallbackContext context) => shooting.SetAiming(true);
    private void OnAimEnd(InputAction.CallbackContext context) => shooting.SetAiming(false);
    private void OnReload(InputAction.CallbackContext context) => shooting.Reload();
    private void OnInteract(InputAction.CallbackContext context) => interaction.HandleInteraction();
    private void OnFireModeSwitch(InputAction.CallbackContext context) => shooting.SwitchFireMode();
    private void OnMelee(InputAction.CallbackContext context) => shooting.Melee();
    
    private void OnGrenadeToggle(InputAction.CallbackContext context)
    {
        if (grenadeSystem != null)
        {
            grenadeSystem.ToggleGrenadeMode();
        }
        else
        {
            shooting.ThrowGrenade();
        }
    }

    private void HandleResumeGame()
    {
        inputPlayer.Enable();
    }

    private void HandlePauseGame()
    {
        inputPlayer.Disable();
    }
}