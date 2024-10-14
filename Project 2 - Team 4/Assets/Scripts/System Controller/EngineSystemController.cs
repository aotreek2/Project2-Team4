using UnityEngine;

public class EngineSystemController : MonoBehaviour
{
    public float engineHealth = 100f;
    public float engineMaxHealth = 100f;
    public float engineEfficiency = 1f;
    public GameObject enginesCube;

    public ParticleSystem engineFireParticles;
    private bool isEngineOnFire = false;
    public MonoBehaviour[] adjacentSystems;

    public float fireIntensity = 0f;
    public float fireSpreadChance = 0.2f;

    public ResourceManager resourceManager;

    void Start()
    {
        if (engineFireParticles != null)
        {
            engineFireParticles.Stop();
        }

        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
        }
    }

    void Update()
    {
        UpdateEngineEfficiency();
        if (isEngineOnFire)
        {
            UpdateFire();
        }
    }

    void UpdateEngineEfficiency()
    {
        engineEfficiency = Mathf.Clamp(engineHealth / engineMaxHealth, 0.1f, 1.0f);

        if (resourceManager != null)
        {
            resourceManager.engineEfficiency = engineEfficiency;
        }
    }

    public void DamageEngine(float damage)
    {
        engineHealth -= damage;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        if (engineHealth <= engineMaxHealth * 0.5f && !isEngineOnFire)
        {
            StartEngineFire();
        }
    }

    public void RepairEngine(float amount)
    {
        engineHealth += amount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        if (engineHealth == engineMaxHealth && isEngineOnFire)
        {
            StopEngineFire();
        }
    }

    public void StartEngineFire()
    {
        if (engineFireParticles != null)
        {
            engineFireParticles.Play();
            isEngineOnFire = true;
            fireIntensity = 1.0f;
            InvokeRepeating("PropagateFire", 5f, 10f);
        }
    }

    public void StopEngineFire()
    {
        if (engineFireParticles != null)
        {
            engineFireParticles.Stop();
            isEngineOnFire = false;
            fireIntensity = 0f;
            CancelInvoke("PropagateFire");
        }
    }

    public void ReduceEngineEfficiency(float percentage)
    {
        float reduction = engineMaxHealth * (percentage / 100f);
        engineHealth -= reduction;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        UpdateEngineEfficiency();
    }

    public void UpdateEngineCubeColor()
    {
        if (enginesCube != null)
        {
            Renderer renderer = enginesCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                float healthPercentage = engineHealth / engineMaxHealth;
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }

    void PropagateFire()
    {
        if (!isEngineOnFire) return;

        foreach (MonoBehaviour adjacentSystem in adjacentSystems)
        {
            if (adjacentSystem is EngineSystemController engineController && !engineController.isEngineOnFire)
            {
                TrySpreadFire(engineController);
            }
            else if (adjacentSystem is HullSystemController hullController && !hullController.isHullOnFire)
            {
                TrySpreadFire(hullController);
            }
        }
    }

    void TrySpreadFire(MonoBehaviour system)
    {
        if (Random.value < fireSpreadChance)
        {
            if (system is EngineSystemController engineController)
            {
                engineController.StartEngineFire();
            }
            else if (system is HullSystemController hullController)
            {
                hullController.StartHullFire();
            }
        }
    }

    void UpdateFire()
    {
        fireIntensity += Time.deltaTime * 0.1f;
        fireIntensity = Mathf.Clamp(fireIntensity, 0f, 10f);

        DealFireDamage(fireIntensity * Time.deltaTime);

        fireSpreadChance = Mathf.Clamp(fireIntensity * 0.05f, 0.1f, 0.8f);
    }

    void DealFireDamage(float damageAmount)
    {
        engineHealth -= damageAmount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        if (engineHealth <= 0)
        {
            engineHealth = 0;
            StopEngineFire();
        }
    }
}
