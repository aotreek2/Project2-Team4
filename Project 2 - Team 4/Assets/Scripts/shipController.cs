using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour
{
    // Existing fields and variables
    public float engineHealth = 100f;
    public float lifeSupportHealth = 100f;
    public float hullIntegrity = 100f;
    public float crewMorale = 100f; // Overall crew morale
    public float generatorHealth = 100f; // Generator health

    public float engineMaxHealth = 100f;
    public float lifeSupportMaxHealth = 100f;
    public float hullMaxIntegrity = 100f;
    public float generatorMaxHealth = 100f; // Max health for generator

    public GameObject lifeSupportCube;
    public GameObject enginesCube;
    public GameObject hullCube;
    public GameObject generatorCube; // Generator cube
    public GameObject[] lights; // Lights affected by generator health
    public int crewCount = 20; // Starting crew count

    public ResourceManager resourceManager;
    public Camera mainCamera;
    public AudioClip hullDamageSound;
    public AudioSource audioSource; // Reference to the main camera
    

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

        if (lights == null || lights.Length == 0)
        {
            Debug.LogError("Lights array is not assigned or empty. Please assign the lights in the inspector.");
        }

        // Initialize cubes' colors
        UpdateSystemCubes();
    }

    void Update()
    {
        UpdateSystems();
        UpdateSystemCubes();
        UpdateLights();
    }

    // Update efficiencies and system statuses
    void UpdateSystems()
    {
        // Update engine efficiency
        float engineHealthPercentage = engineHealth / engineMaxHealth;
        resourceManager.engineEfficiency = Mathf.Clamp(engineHealthPercentage, 0.1f, 1.2f);

        // Update life support efficiency
        float lifeSupportHealthPercentage = lifeSupportHealth / lifeSupportMaxHealth;
        resourceManager.lifeSupportEfficiency = Mathf.Clamp(lifeSupportHealthPercentage, 0.1f, 1.2f);

        // Update generator efficiency
        float generatorHealthPercentage = generatorHealth / generatorMaxHealth;
        resourceManager.generatorEfficiency = Mathf.Clamp(generatorHealthPercentage, 0.1f, 1.2f);

        // Check for generator failure
        if (generatorHealth <= 0f)
        {
            Debug.Log("Generator is down! Systems are losing power.");
            ReduceLifeSupportEfficiency(50f); // Reduce life support efficiency if generator fails
            ShakeCamera(0.5f, 1.0f); // Trigger camera shake when generator fails
            FlickerLights(1.0f); // Trigger lights flickering when generator fails
        }
    }

    void UpdateSystemCubes()
    {
        UpdateCubeColor(lifeSupportCube, lifeSupportHealth / lifeSupportMaxHealth);
        UpdateCubeColor(enginesCube, engineHealth / engineMaxHealth);
        UpdateCubeColor(hullCube, hullIntegrity / hullMaxIntegrity);
        UpdateCubeColor(generatorCube, generatorHealth / generatorMaxHealth);
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

    void UpdateLights()
    {
        bool lightsOn = generatorHealth > 0;
        foreach (GameObject lightObject in lights)
        {
            if (lightObject != null)
            {
                Light lightComponent = lightObject.GetComponent<Light>();
                if (lightComponent != null)
                {
                    if (lightComponent.type == LightType.Directional)
                    {
                        // Instead of enabling/disabling directional light, change its intensity
                        lightComponent.intensity = lightsOn ? 1.0f : 0.0f;
                    }
                    else
                    {
                        // For other light types, enable/disable them as usual
                        lightComponent.enabled = lightsOn;
                    }
                }
            }
        }
    }

    public void FlickerLights(float flickerDuration)
    {
        Debug.Log("FlickerLights called with duration: " + flickerDuration);
        StartCoroutine(FlickerLightsCoroutine(flickerDuration));
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

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.unscaledDeltaTime; // Use unscaled delta time

            yield return null;
        }

        mainCamera.transform.localPosition = originalPosition;
    }


    private IEnumerator FlickerLightsCoroutine(float duration)
    {
        if (lights == null || lights.Length == 0)
        {
            Debug.LogError("Lights array is null or empty. Cannot flicker lights.");
            yield break;
        }

        float elapsed = 0f;
        bool lightsOn = true;

        while (elapsed < duration)
        {
            lightsOn = !lightsOn;
            foreach (GameObject lightObject in lights)
            {
                if (lightObject != null)
                {
                    Light lightComponent = lightObject.GetComponent<Light>();
                    if (lightComponent != null)
                    {
                        lightComponent.enabled = lightsOn;
                    }
                }
            }

            elapsed += duration / 6f; // Adjust flicker rate as needed

            yield return new WaitForSecondsRealtime(duration / 6f); // Use unscaled time
        }

        // Ensure lights are on after flickering
        foreach (GameObject lightObject in lights)
        {
            if (lightObject != null)
            {
                Light lightComponent = lightObject.GetComponent<Light>();
                if (lightComponent != null)
                {
                    lightComponent.enabled = true;
                }
            }
        }
    }

    // Repair system method
    public void RepairSystem(CubeInteraction.SystemType systemType, float repairAmount)
    {
        switch (systemType)
        {
            case CubeInteraction.SystemType.Engines:
                engineHealth += repairAmount;
                engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
                Debug.Log("Engine repaired by " + repairAmount + " points.");
                break;

            case CubeInteraction.SystemType.LifeSupport:
                lifeSupportHealth += repairAmount;
                lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
                Debug.Log("Life Support repaired by " + repairAmount + " points.");
                break;

            case CubeInteraction.SystemType.Hull:
                hullIntegrity += repairAmount;
                hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
                Debug.Log("Hull repaired by " + repairAmount + " points.");
                break;

            case CubeInteraction.SystemType.Generator:
                generatorHealth += repairAmount;
                generatorHealth = Mathf.Clamp(generatorHealth, 0f, generatorMaxHealth);
                Debug.Log("Generator repaired by " + repairAmount + " points.");
                break;
        }
    }

    // Methods for damaging and repairing systems
    public void DamageEngine(float damage)
    {
        engineHealth -= damage;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
        Debug.Log("Engine damaged by " + damage + " points.");

        // Trigger effects when engine is damaged
        ShakeCamera(0.5f, 1.0f);
        FlickerLights(0.5f);
    }

    public void RepairEngine(float amount)
    {
        engineHealth += amount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
        Debug.Log("Engine repaired by " + amount + " points.");
    }

    public void DamageLifeSupport(float damage)
    {
        lifeSupportHealth -= damage;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log("Life Support damaged by " + damage + " points.");

        // Trigger effects when life support is damaged
        ShakeCamera(0.5f, 1.0f);
        FlickerLights(0.5f);
    }

    public void RepairLifeSupport(float amount)
    {
        lifeSupportHealth += amount;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log("Life Support repaired by " + amount + " points.");
    }

    public void DamageHull(float damage)
    {
        hullIntegrity -= damage;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
        Debug.Log("Hull damaged by " + damage + " points.");

        // Play sound effect
        if (audioSource != null && hullDamageSound != null)
        {
            audioSource.PlayOneShot(hullDamageSound);
        }

        // Trigger effects when hull is damaged
        ShakeCamera(0.5f, 1.0f);
        FlickerLights(0.5f);
    }

    public void RepairHull(float amount)
    {
        hullIntegrity += amount;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
        Debug.Log("Hull repaired by " + amount + " points.");
    }

    public void DamageGenerator(float damage)
    {
        generatorHealth -= damage;
        generatorHealth = Mathf.Clamp(generatorHealth, 0f, generatorMaxHealth);
        Debug.Log("Generator damaged by " + damage + " points.");

        // Trigger effects when generator is damaged
        ShakeCamera(0.5f, 1.0f);
        FlickerLights(0.5f);
    }

    public void RepairGenerator(float amount)
    {
        generatorHealth += amount;
        generatorHealth = Mathf.Clamp(generatorHealth, 0f, generatorMaxHealth);
        Debug.Log("Generator repaired by " + amount + " points.");
    }

    // Methods to support DecisionPanelManager

    // Sacrifice crew members to stabilize systems
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

    // Reduce the hull integrity by a percentage
    public void ReduceHullIntegrity(float percentage)
    {
        float reduction = hullMaxIntegrity * (percentage / 100f);
        hullIntegrity -= reduction;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
        Debug.Log($"Hull integrity reduced by {percentage}%.");
        ShakeCamera(0.5f, 1.0f); // Trigger camera shake when hull integrity is reduced
    }

    // Reduce life support system's efficiency
    public void ReduceLifeSupportEfficiency(float percentage)
    {
        float reduction = lifeSupportMaxHealth * (percentage / 100f);
        lifeSupportHealth -= reduction;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log($"Life Support efficiency reduced by {percentage}%.");
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

    public void SacrificeCrewForRepair(int amount, CubeInteraction.SystemType systemType)
    {
        if (crewCount >= amount)
        {
            crewCount -= amount;
            Debug.Log($"{amount} crew members sacrificed to repair {systemType}.");

            // Instantly repair the system
            RepairSystem(systemType, 100f);
            AdjustCrewMorale(-20f); // Decrease morale due to sacrifice
        }
        else
        {
            Debug.Log("Not enough crew members to sacrifice.");
        }
    }

    public void AddCrew(int amount)
    {
        crewCount += amount;
        Debug.Log($"{amount} crew members added. Total crew: {crewCount}.");
    }

}
