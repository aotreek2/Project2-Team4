// ShipController.cs
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Added to resolve SceneManager errors

public class ShipController : MonoBehaviour
{
    [Header("Crew and Morale")]
    public float crewMorale = 100f; // Overall crew morale

    [Header("Resources")]
    public ResourceManager resourceManager;
    // Removed duplicate crewCount if present
    // public float fuelAmount = 100f; // Starting fuel (Optional: If managed by ResourceManager)

    [Header("Audio Components")]
    public AudioClip hullDamageSound;
    public AudioSource audioSource;

    [Header("System Controllers")]
    public HullSystemController hullSystemController;
    public GeneratorController generatorController;
    public LifeSupportController lifeSupportController;
    public EngineSystemController engineSystemController;

    [Header("Camera and Decisions")]
    public CameraController cameraController; // Reference to CameraController
    public DecisionController decisionController; // Reference to the DecisionController
    public ChapterManager chapterManager;

    [Header("Ship Health")]
    private float shipHealth = 100f;
    private float shipMaxHealth = 100f;
    public float criticalThreshold = 30f; // Health below this percentage is considered critical

    void Start()
    {
        // Initialize cameraController
        cameraController = FindObjectOfType<CameraController>();

        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogError("[ShipController] ResourceManager not found in the scene.");
            }
        }

        // Initialize system controllers
        InitializeControllers();

        // Ensure the decision controller is set
        if (decisionController == null)
        {
            decisionController = FindObjectOfType<DecisionController>();
            if (decisionController == null)
            {
                Debug.LogError("[ShipController] DecisionController not found in the scene.");
            }
        }

        // Initialize cubes' colors
        UpdateSystemCubes();

        // Initialize ChapterManager
        if (chapterManager == null)
        {
            chapterManager = FindObjectOfType<ChapterManager>();
            if (chapterManager == null)
            {
                Debug.LogError("[ShipController] ChapterManager not found in the scene.");
            }
        }
    }

    void Update()
    {
        UpdateSystems();
        UpdateSystemCubes();
        UpdateShipHealth();
        CheckForCriticalSystems();
    }

    // Initialize system controllers
    private void InitializeControllers()
    {
        if (hullSystemController == null)
        {
            hullSystemController = FindObjectOfType<HullSystemController>();
            if (hullSystemController == null)
            {
                Debug.LogError("[ShipController] HullSystemController not found in the scene.");
            }
        }

        if (generatorController == null)
        {
            generatorController = FindObjectOfType<GeneratorController>();
            if (generatorController == null)
            {
                Debug.LogError("[ShipController] GeneratorController not found in the scene.");
            }
        }

        if (lifeSupportController == null)
        {
            lifeSupportController = FindObjectOfType<LifeSupportController>();
            if (lifeSupportController == null)
            {
                Debug.LogError("[ShipController] LifeSupportController not found in the scene.");
            }
        }

        if (engineSystemController == null)
        {
            engineSystemController = FindObjectOfType<EngineSystemController>();
            if (engineSystemController == null)
            {
                Debug.LogError("[ShipController] EngineSystemController not found in the scene.");
            }
        }
    }

    // Update efficiencies and system statuses
    void UpdateSystems()
    {
        // Update generator efficiency via GeneratorController
        if (generatorController != null && resourceManager != null)
        {
            // Example fuel consumption
            resourceManager.AdjustFuel(-0.1f * Time.deltaTime);

            // Example: Update generator efficiency based on generator health
            // Uncomment and adjust if GeneratorController has these properties
            // resourceManager.generatorEfficiency = Mathf.Clamp(generatorController.generatorHealth / generatorController.generatorMaxHealth, 0.1f, 1.2f);
        }

        // Check for generator failure
        if (generatorController != null && generatorController.generatorHealth <= 0f)
        {
            ShakeCamera(0.5f, 1.0f); // Trigger camera shake when generator fails
            AlertManager.Instance?.ShowAlert("Generator has failed!");
        }
    }

    void UpdateSystemCubes()
    {
        if (engineSystemController != null)
        {
            engineSystemController.UpdateEngineCubeColor();
        }

        if (generatorController != null)
        {
            UpdateCubeColor(generatorController.gameObject, generatorController.generatorHealth / generatorController.generatorMaxHealth);
        }

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

    /// <summary>
    /// Triggers a camera shake effect.
    /// </summary>
    /// <param name="duration">Duration of the shake in seconds.</param>
    /// <param name="magnitude">Intensity of the shake.</param>
    public void ShakeCamera(float duration, float magnitude)
    {
        if (cameraController != null)
        {
            cameraController.ShakeCamera(duration, magnitude);
        }
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

    /// <summary>
    /// Sacrifices a specified number of crew members.
    /// </summary>
    /// <param name="amount">Number of crew members to sacrifice.</param>
    public void SacrificeCrew(int amount)
    {
        if (resourceManager != null)
        {
            resourceManager.SacrificeCrew(amount);
            AdjustCrewMorale(-10f); // Decrease morale due to sacrifice

            // Optionally, play a sound or trigger an alert
            if (audioSource != null && hullDamageSound != null)
            {
                audioSource.PlayOneShot(hullDamageSound);
                Debug.Log($"[SacrificeCrew] Played hull damage sound due to sacrificing crew.");
            }

            Debug.Log($"[SacrificeCrew] Sacrificed {amount} crew members. Remaining crew: {resourceManager.crewCount}");

            // Trigger camera shake
            ShakeCamera(0.5f, 0.5f);

            // Show alert for crew sacrifice
            AlertManager.Instance?.ShowAlert($"Sacrificed {amount} crew members.");

            // Check for ship destruction
            if (shipHealth <= 0f)
            {
                DestroyShip();
            }
        }
        else
        {
            Debug.LogError("[SacrificeCrew] ResourceManager is not assigned.");
        }
    }

    /// <summary>
    /// Adjusts the crew morale by a specified amount.
    /// </summary>
    /// <param name="amount">Amount to adjust the morale by.</param>
    public void AdjustCrewMorale(float amount)
    {
        crewMorale += amount;
        crewMorale = Mathf.Clamp(crewMorale, 0f, 100f);

        // Update morale for each crew member
        CrewMember[] crewMembers = FindObjectsOfType<CrewMember>();
        foreach (CrewMember crew in crewMembers)
        {
            crew.AdjustMorale(amount);
        }

        Debug.Log($"[ShipController] Crew morale adjusted by {amount}. Current morale: {crewMorale}");

        // Optionally, trigger an alert if morale is too low
        if (crewMorale < 30f)
        {
            Debug.LogWarning("[ShipController] Crew morale is critically low!");
            AlertManager.Instance?.ShowAlert("Crew morale is critically low!");
        }
    }

    /// <summary>
    /// Adds a specified number of crew members.
    /// </summary>
    /// <param name="amount">Number of crew members to add.</param>
    public void AddCrew(int amount)
    {
        if (resourceManager != null)
        {
            resourceManager.crewCount += amount;
            resourceManager.UpdateResourceUI();
            Debug.Log($"[AddCrew] Added {amount} crew members. Total crew: {resourceManager.crewCount}");
        }
        else
        {
            Debug.LogError("[AddCrew] ResourceManager is not assigned.");
        }
    }

    /// <summary>
    /// Applies hull damage to the ship.
    /// </summary>
    /// <param name="damageAmount">Amount of hull damage to apply.</param>
    public void ApplyHullDamage(float damageAmount)
    {
        if (hullSystemController != null)
        {
            hullSystemController.DamageHull(damageAmount);
            Debug.Log($"[ShipController] Applied {damageAmount}% hull damage. Current hull health: {hullSystemController.hullHealth}%.");

            // Play hull damage sound
            if (audioSource != null && hullDamageSound != null)
            {
                audioSource.PlayOneShot(hullDamageSound);
                Debug.Log("[ShipController] Hull damage sound played.");
            }

            // Trigger camera shake
            ShakeCamera(0.3f, 0.7f);

            // Show alert for hull damage
            AlertManager.Instance?.ShowAlert($"Hull damaged by {damageAmount}%!");
        }
        else
        {
            Debug.LogError("[ApplyHullDamage] HullSystemController is not assigned.");
        }
    }

    /// <summary>
    /// Sacrifices crew members to repair a specific system.
    /// </summary>
    /// <param name="crewAmount">Number of crew members to sacrifice.</param>
    /// <param name="systemType">Type of system to repair.</param>
    public void SacrificeCrewForRepair(int crewAmount, CubeInteraction.SystemType systemType)
    {
        if (resourceManager != null)
        {
            if (resourceManager.crewCount >= crewAmount)
            {
                resourceManager.SacrificeCrew(crewAmount);
                Debug.Log($"[ShipController] Sacrificed {crewAmount} crew members for repairing {systemType}.");

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

                // Trigger camera shake
                ShakeCamera(0.5f, 1.0f);

                // Show alert for crew sacrifice
                AlertManager.Instance?.ShowAlert($"Sacrificed {crewAmount} crew members to repair {systemType}.");

                // Check for ship destruction
                if (shipHealth <= 0f)
                {
                    DestroyShip();
                }
            }
            else
            {
                Debug.LogWarning("[ShipController] Not enough crew to sacrifice for repair.");
                AlertManager.Instance?.ShowAlert("Not enough crew to sacrifice for repair.");
            }
        }
        else
        {
            Debug.LogError("[ShipController] ResourceManager is not assigned.");
        }
    }

    /// <summary>
    /// Checks for critical system statuses and triggers decisions if necessary.
    /// </summary>
    public void CheckForCriticalSystems()
    {
        if (engineSystemController != null)
        {
            // Trigger decision panel when health is critically low, but not zero
            if (engineSystemController.engineHealth <= engineSystemController.engineMaxHealth * 0.3f && engineSystemController.engineHealth > 0)
            {
                if (chapterManager.currentChapter == ChapterManager.Chapter.Chapter1)
                {
                    return;
                }
                else
                {
                    decisionController.ShowDecision(
                        "The engine is critically damaged! Sacrifice 3 crew members to repair the engine?",
                        () => SacrificeCrewForRepair(3, CubeInteraction.SystemType.Engines),
                        () => { /* Option 2 can be handled here if needed */ }
                    );
                }
            }
            else if (engineSystemController.engineHealth == 0)
            {
                if (chapterManager.currentChapter == ChapterManager.Chapter.Chapter1)
                {
                    return;
                }
                else
                {
                    decisionController.ShowDecision(
                        "The engine has completely failed! Sacrifice 5 crew members to restart the engine?",
                        () => SacrificeCrewForRepair(5, CubeInteraction.SystemType.Engines),
                        () => { /* Option 2 can be handled here if needed */ }
                    );
                }
            }
        }
    }

    /// <summary>
    /// Updates the ship's overall health based on individual system health.
    /// </summary>
    void UpdateShipHealth()
    {
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
    }

    /// <summary>
    /// Determines if the ship is in a critical condition.
    /// </summary>
    /// <returns>True if ship health is below or equal to the critical threshold.</returns>
    public bool IsCriticalCondition()
    {
        float healthPercentage = (shipHealth / shipMaxHealth) * 100f;
        return healthPercentage <= criticalThreshold;
    }

    /// <summary>
    /// Gets the current ship health percentage.
    /// </summary>
    /// <returns>Ship health percentage.</returns>
    public float GetShipHealthPercentage()
    {
        return (shipHealth / shipMaxHealth) * 100f;
    }

    /// <summary>
    /// Adds biomass fuel to the engine system.
    /// </summary>
    /// <param name="biomassAmount">Amount of biomass to add.</param>
    public void AddBiomassFuel(float biomassAmount)
    {
        if (engineSystemController != null)
        {
            engineSystemController.AddFuel(biomassAmount); // Now biomass goes to the engine system
            Debug.Log($"[AddBiomassFuel] Added {biomassAmount} biomass fuel to engines.");
        }
    }

    /// <summary>
    /// Destroys the ship and triggers game over.
    /// </summary>
    private void DestroyShip()
    {
        Debug.Log("[ShipController] Ship has been destroyed! Game Over.");
        // Implement game over logic here, such as loading a Game Over scene
        SceneManager.LoadScene("GameOverScene");
    }
}
