using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour
{
    // Existing fields and variables
    public float crewMorale = 100f; // Overall crew morale
    public int crewCount = 20; // Starting crew count

    public GameObject generatorCube; // Generator cube
    public ResourceManager resourceManager;
    public Camera mainCamera;
    public AudioClip hullDamageSound;
    public AudioSource audioSource; // Reference to the main camera

    // References to system controllers
    public HullSystemController hullSystemController;
    public GeneratorController generatorController;
    public LifeSupportController lifeSupportController;
    public EngineSystemController engineSystemController;

    // Ship health variables
    private float shipHealth = 100f;
    private float shipMaxHealth = 100f;
    public float criticalThreshold = 30f; // Health below this percentage is considered critical

    void Start()
    {
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogError("ResourceManager not found in the scene. Please ensure there is a ResourceManager script in the scene.");
            }
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is not assigned and cannot be found in the scene. Please assign the Main Camera in the inspector.");
            }
        }

        // Initialize system controllers
        InitializeControllers();

        // Initialize cubes' colors
        UpdateSystemCubes();
    }

    void Update()
    {
        UpdateSystems();
        UpdateSystemCubes();
        UpdateShipHealth();
    }

    // Initialize controllers
    private void InitializeControllers()
    {
        if (hullSystemController == null)
        {
            hullSystemController = FindObjectOfType<HullSystemController>();
            if (hullSystemController == null)
            {
                Debug.LogError("HullSystemController not found. Ensure it's properly set in the scene.");
            }
        }

        if (generatorController == null)
        {
            generatorController = FindObjectOfType<GeneratorController>();
            if (generatorController == null)
            {
                Debug.LogError("GeneratorController not found in the scene. Please assign it in the inspector.");
            }
        }

        if (lifeSupportController == null)
        {
            lifeSupportController = FindObjectOfType<LifeSupportController>();
            if (lifeSupportController == null)
            {
                Debug.LogError("LifeSupportController not found. Ensure it's properly set in the scene.");
            }
        }

        if (engineSystemController == null)
        {
            engineSystemController = FindObjectOfType<EngineSystemController>();
            if (engineSystemController == null)
            {
                Debug.LogError("EngineSystemController not found. Ensure it's properly set in the scene.");
            }
        }
    }

    // Update efficiencies and system statuses
    void UpdateSystems()
    {
        // Update generator efficiency via GeneratorController
        if (generatorController != null)
        {
            resourceManager.generatorEfficiency = Mathf.Clamp(generatorController.generatorHealth / generatorController.generatorMaxHealth, 0.1f, 1.2f);
        }

        // Check for generator failure
        if (generatorController != null && generatorController.generatorHealth <= 0f)
        {
            Debug.Log("Generator is down! Systems are losing power.");
            ShakeCamera(0.5f, 1.0f); // Trigger camera shake when generator fails
        }
    }

    void UpdateSystemCubes()
    {
        if (engineSystemController != null)
        {
            engineSystemController.UpdateEngineCubeColor();
        }

        UpdateCubeColor(generatorCube, generatorController != null ? generatorController.generatorHealth / generatorController.generatorMaxHealth : 0);

        // Call the UpdateCubeColor method in HullSystemController
        if (hullSystemController != null)
        {
            hullSystemController.UpdateHullCubeColor();
        }
    }

    void UpdateCubeColor(GameObject cube, float healthPercentage)
    {
        if (cube != null)
        {
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }
    public void ShakeCamera(float duration, float magnitude)
    {
        Debug.Log("ShakeCamera called with duration: " + duration + ", magnitude: " + magnitude);
        StartCoroutine(ShakeCameraCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCameraCoroutine(float duration, float magnitude)
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is null. Cannot shake camera.");
            yield break;
        }

        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0.0f;
        float damping = 1.0f; // Value to reduce shaking over time

        while (elapsed < duration)
        {
            // Reduce the magnitude gradually to make the shake smoother (damping effect)
            float currentMagnitude = magnitude * (1.0f - (elapsed / duration)) * damping;

            // Generate random shake values
            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float y = Random.Range(-1f, 1f) * currentMagnitude;

            // Apply shake to the camera
            mainCamera.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return the camera to its original position after shaking
        mainCamera.transform.localPosition = originalPosition;
    }
    // Forward engine-related calls to EngineSystemController
    public void DamageEngine(float damage)
    {
        if (engineSystemController != null)
        {
            engineSystemController.DamageEngine(damage);
        }
    }

    public void RepairEngine(float amount)
    {
        if (engineSystemController != null)
        {
            engineSystemController.RepairEngine(amount);
        }
    }

    // Delegate generator damage to GeneratorController
    public void DamageGenerator(float damage)
    {
        if (generatorController != null)
        {
            generatorController.DamageGenerator(damage);
        }
    }

    // Delegate generator repair to GeneratorController
    public void RepairGenerator(float amount)
    {
        if (generatorController != null)
        {
            generatorController.RepairGenerator(amount);
        }
    }

    public void SacrificeCrew(int amount)
    {
        if (crewCount >= amount)
        {
            crewCount -= amount;
            Debug.Log($"{amount} crew members sacrificed.");
            AdjustCrewMorale(-10f); // Decrease morale due to sacrifice
        }
        else
        {
            Debug.Log("Not enough crew members to sacrifice.");
        }
    }

    public void AdjustCrewMorale(float amount)
    {
        crewMorale += amount;
        crewMorale = Mathf.Clamp(crewMorale, 0f, 100f);
        Debug.Log($"Crew morale adjusted by {amount}. Current morale: {crewMorale}%");

        // Update morale for each crew member
        CrewMember[] crewMembers = FindObjectsOfType<CrewMember>();
        foreach (CrewMember crew in crewMembers)
        {
            crew.AdjustMorale(amount);
        }
    }

    public void AddCrew(int amount)
    {
        crewCount += amount;
        Debug.Log($"{amount} crew members added. Total crew: {crewCount}.");
    }

    public void DamageHull(float damageAmount)
    {
        if (hullSystemController != null)
        {
            hullSystemController.DamageHull(damageAmount);
        }
    }

    public void RepairHull(float repairAmount)
    {
        if (hullSystemController != null)
        {
            hullSystemController.RepairHull(repairAmount);
        }
    }

    public void SacrificeCrewForRepair(int crewAmount, CubeInteraction.SystemType systemType)
    {
        if (crewCount >= crewAmount)
        {
            crewCount -= crewAmount;
            Debug.Log($"{crewAmount} crew members sacrificed to repair {systemType}.");

            // Repair the system fully
            switch (systemType)
            {
                case CubeInteraction.SystemType.Engines:
                    engineSystemController.RepairEngine(engineSystemController.engineMaxHealth - engineSystemController.engineHealth); // Repair the missing health amount
                    break;
                case CubeInteraction.SystemType.Hull:
                    hullSystemController.RepairHull(hullSystemController.hullMaxHealth - hullSystemController.hullHealth); // Repair the missing health amount
                    break;
                case CubeInteraction.SystemType.LifeSupport:
                    lifeSupportController.RepairLifeSupport(lifeSupportController.lifeSupportMaxHealth - lifeSupportController.lifeSupportHealth); // Repair the missing health amount
                    break;
                case CubeInteraction.SystemType.Generator:
                    RepairGenerator(generatorController.generatorMaxHealth - generatorController.generatorHealth); // Repair the missing health amount
                    break;
            }

            AdjustCrewMorale(-20f); // Decrease morale due to sacrifice
        }
        else
        {
            Debug.Log("Not enough crew members to sacrifice.");
        }
    }

    // **Added Methods for Chapter System**

    public void DamageShipAtStart()
    {
        // Damage critical systems at the start of the game
        lifeSupportController.DamageLifeSupport(50f);
        engineSystemController.DamageEngine(50f);
        hullSystemController.DamageHull(50f);
        Debug.Log("Critical systems have been damaged at the start.");
    }

    public bool AreCriticalSystemsRepaired()
    {
        float tolerance = 0.01f; // Define a small tolerance for float comparison

        bool lifeSupportRepaired = Mathf.Abs(lifeSupportController.lifeSupportHealth - lifeSupportController.lifeSupportMaxHealth) < tolerance;
        bool engineRepaired = Mathf.Abs(engineSystemController.engineHealth - engineSystemController.engineMaxHealth) < tolerance;
        bool hullRepaired = Mathf.Abs(hullSystemController.hullHealth - hullSystemController.hullMaxHealth) < tolerance;

        return lifeSupportRepaired && engineRepaired && hullRepaired;
    }

    // **New Methods and Modifications**

    // Method to update ship's overall health based on system health
    void UpdateShipHealth()
    {
        // Calculate the average health of all systems
        float totalHealthPercentage = 0f;
        int systemCount = 0;

        if (lifeSupportController != null)
        {
            totalHealthPercentage += lifeSupportController.lifeSupportHealth / lifeSupportController.lifeSupportMaxHealth;
            systemCount++;
        }

        if (engineSystemController != null)
        {
            totalHealthPercentage += engineSystemController.engineHealth / engineSystemController.engineMaxHealth;
            systemCount++;
        }

        if (hullSystemController != null)
        {
            totalHealthPercentage += hullSystemController.hullHealth / hullSystemController.hullMaxHealth;
            systemCount++;
        }

        if (generatorController != null)
        {
            totalHealthPercentage += generatorController.generatorHealth / generatorController.generatorMaxHealth;
            systemCount++;
        }

        if (systemCount > 0)
        {
            float averageHealthPercentage = totalHealthPercentage / systemCount;
            shipHealth = averageHealthPercentage * shipMaxHealth;
        }
        else
        {
            shipHealth = shipMaxHealth;
        }

        // Debug.Log($"Ship Health: {shipHealth}/{shipMaxHealth} ({(shipHealth / shipMaxHealth) * 100f}%)");
    }

    // Method to determine if the ship is in critical condition
    public bool IsCriticalCondition()
    {
        // Ship is in critical condition if shipHealth is below criticalThreshold percentage
        float healthPercentage = (shipHealth / shipMaxHealth) * 100f;
        return healthPercentage <= criticalThreshold;
    }

    // Method to get ship's current health percentage
    public float GetShipHealthPercentage()
    {
        return (shipHealth / shipMaxHealth) * 100f;
    }
}
