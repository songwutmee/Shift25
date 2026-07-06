using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem; // [New Input System] Required for Keyboard check

// [Singleton Pattern] Manages the global stress level (Pressure) of the player.
public class PressureManager : MonoBehaviour
{
    public static PressureManager Instance { get; private set; }

    [Header("Pressure Stats")]
    [SerializeField] private float currentPressure = 0f;
    [SerializeField] private float maxPressure = 100f;

    private bool isGameRunning = true;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() => StartPressureTick().Forget();

    private void Update()
    {
        // [Debug Tooling] Press 'P' to manipulate pressure for testing
        // Requirement: P once -> 70%, P again -> 100%
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            HandleDebugPressure();
        }
    }

    private void HandleDebugPressure()
    {
        float threshold70 = maxPressure * 0.7f;
        
        if (currentPressure < threshold70)
        {
            // Jump to 70% to test Red Shader / Eyes
            AddPressure(threshold70 - currentPressure);
            Debug.Log("<color=yellow>[Debug]</color> Pressure jumped to 70%");
        }
        else
        {
            // Jump to 100% to test the 25:00 Ending
            AddPressure(maxPressure - currentPressure);
            Debug.Log("<color=red>[Debug]</color> Pressure jumped to 100%");
        }
    }

    public float GetCurrentPressure() => currentPressure;
    public float GetMaxPressure() => maxPressure;

    private async UniTaskVoid StartPressureTick()
    {
        while (isGameRunning)
        {
            AddPressure(0.05f); // Reduced default tick for 1-hour gameplay
            await UniTask.Delay(1000); 
        }
    }

    public void AddPressure(float amount)
    {
        currentPressure = Mathf.Clamp(currentPressure + amount, 0, maxPressure);
        GameEvents.RaisePressureChanged(currentPressure, maxPressure);

        if (currentPressure >= maxPressure) TriggerMentalCollapse();
    }

    private void TriggerMentalCollapse()
    {
        if (!isGameRunning) return;
        isGameRunning = false;
        Debug.Log("[System] Mental Collapse Reached.");
    }
}