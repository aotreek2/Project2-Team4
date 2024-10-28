using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour
{
    // Existing fields and variables
    public float crewMorale = 100f; // Overall crew morale
    public int crewCount = 20; // Starting crew count

    public GameObject generatorCube; // Generator cube
    public ResourceManager resourceManager;
    public AudioClip hullDamageSound;
    public AudioSource audioSource;

    // References to system controllers
    public HullSystemController hullSystemController;
    public GeneratorController generatorController;
    public LifeSupportController lifeSupportController;
    public EngineSystemController engineSystemController;
    public CameraController cameraController; // Reference to CameraController
    public DecisionController decisionController; // Reference to the DecisionController
    public ChapterManager chapterManager;

    // Ship health variables
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
        }

        // Initialize system controllers
        InitializeControllers();

        // Ensure the decision controller is set
        if (decisionController == null)
        {
            decisionController = FindObjectOfType<DecisionController>();
        }

        // Initialize cubes' colors
        UpdateSystemCubes();

        chapterManager = chapterManager ?? FindObjectOfType<ChapterManager>();
    }

    void Update()
    {
        UpdateSystems();
        UpdateSystemCubes();
        UpdateShipHealth();
        CheckForCriticalSystems();
    }

    // Initialize controllers
    private void InitializeControllers()
    {
        if (hullSystemController == null)
        {
            hullSystemController = FindObjectOfType<HullSystemController>();
        }

        if (generatorController == null)
        {
            generatorController = FindObjectOfType<GeneratorController>();
        }

        if (lifeSupportController == null)
        {
            lifeSupportController = FindObjectOfType<LifeSupportController>();
        }

        if (engineSystemController == null)
        {
            engineSystemController = FindObjectOfType<EngineSystemController>();
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

    // Corrected ShakeCamera method
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

    public void SacrificeCrew(int amount)
    {
        if (crewCount >= amount)
        {
            crewCount -= amount;
            AdjustCrewMorale(-10f); // Decrease morale due to sacrifice
        }
    }

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
    }

    public void AddCrew(int amount)
    {
        crewCount += amount;
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
    }

    // **New Methods**

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
                    null
                );
              }
            }
            else if (engineSystemController.engineHealth == 0)
            {
                if(chapterManager.currentChapter == ChapterManager.Chapter.Chapter1)
                {
                    return;
                }
                else
                {
                    decisionController.ShowDecision(
                    "The engine has completely failed! Sacrifice 5 crew members to restart the engine?",
                    () => SacrificeCrewForRepair(5, CubeInteraction.SystemType.Engines),
                    null
                );
                }
            }
        }
    }

    // Method to update ship's overall health based on system health
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

    public bool IsCriticalCondition()
    {
        float healthPercentage = (shipHealth / shipMaxHealth) * 100f;
        return healthPercentage <= criticalThreshold;
    }

    public float GetShipHealthPercentage()
    {
        return (shipHealth / shipMaxHealth) * 100f;
    }

    public void AddBiomassFuel(float biomassAmount)
    {
        if (engineSystemController != null)
        {
            engineSystemController.AddFuel(biomassAmount); // Now biomass goes to the engine system
        }
    }

        public void ApplyHullDamage(float damageAmount)
    {
        if (hullSystemController != null)
        {
            hullSystemController.DamageHull(damageAmount);
            Debug.Log($"[ShipController] Applied {damageAmount}% hull damage. Current hull health: {hullSystemController.hullHealth}%.");
        }
        else
        {
            Debug.LogError("[ShipController] HullSystemController is not assigned.");
        }
    }
}
