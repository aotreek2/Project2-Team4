using UnityEngine;

public class HullSystemController : MonoBehaviour
{
    public float hullHealth = 100f;
    public float hullMaxHealth = 100f;
    public GameObject hullCube;

    public ParticleSystem hullFireParticles;
    public ParticleSystem[] hullSmokeParticlesArray;
    public bool isHullOnFire = false;

    private bool isTakingDamageOverTime = false;
    private float damageOverTimeRate = 0f;

    public ResourceManager resourceManager;

    void Start()
    {
        if (hullFireParticles != null)
        {
            hullFireParticles.Stop();
        }

        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null)
            {
                smoke.Stop();
            }
        }

        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
        }
    }

    void Update()
    {
        if (isHullOnFire)
        {
            UpdateFire();
        }

        if (isTakingDamageOverTime)
        {
            float damage = damageOverTimeRate * Time.deltaTime;
            DamageHull(damage);

            if (hullHealth <= 0f)
            {
                isTakingDamageOverTime = false;
            }
        }
    }

    public void StartDamageOverTime(float damageRate)
    {
        isTakingDamageOverTime = true;
        damageOverTimeRate = damageRate;

        if (!isHullOnFire)
        {
            StartHullFire();
        }
    }

    public void StopDamageOverTime()
    {
        isTakingDamageOverTime = false;
    }

    public void DamageHull(float damage)
    {
        hullHealth -= damage;
        hullHealth = Mathf.Clamp(hullHealth, 0f, hullMaxHealth);

        UpdateSmokeEffects();
        UpdateHullCubeColor();
    }

    public void RepairHull(float amount)
    {
        hullHealth += amount;
        hullHealth = Mathf.Clamp(hullHealth, 0f, hullMaxHealth);

        if (hullHealth >= hullMaxHealth)
        {
            StopHullFire();
        }

        UpdateSmokeEffects();
        UpdateHullCubeColor();
    }

    public void ReduceHullIntegrity(float percentage)
    {
        float reductionAmount = hullMaxHealth * (percentage / 100f);
        DamageHull(reductionAmount);
    }

    public void StartHullFire()
    {
        if (hullFireParticles != null)
        {
            hullFireParticles.Play();
            isHullOnFire = true;
        }

        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null && !smoke.isPlaying)
            {
                smoke.Play();
            }
        }
    }

    public void StopHullFire()
    {
        if (hullFireParticles != null)
        {
            hullFireParticles.Stop();
            isHullOnFire = false;
        }

        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null && smoke.isPlaying)
            {
                smoke.Stop();
            }
        }
    }

    public void UpdateHullCubeColor()
    {
        if (hullCube != null)
        {
            Renderer renderer = hullCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                float healthPercentage = hullHealth / hullMaxHealth;
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }

    private void UpdateFire()
    {
        float fireDamage = Time.deltaTime * 5f;
        DamageHull(fireDamage);

        if (hullFireParticles != null)
        {
            float healthPercentage = hullHealth / hullMaxHealth;
            var emission = hullFireParticles.emission;
            emission.rateOverTime = Mathf.Lerp(10f, 50f, 1f - healthPercentage);
        }
    }

    private void UpdateSmokeEffects()
    {
        if (hullSmokeParticlesArray.Length == 0) return;

        float healthPercentage = hullHealth / hullMaxHealth;

        bool hullFullyRepaired = Mathf.Approximately(healthPercentage, 1f);

        if (hullFullyRepaired)
        {
            foreach (ParticleSystem smoke in hullSmokeParticlesArray)
            {
                if (smoke != null && smoke.isPlaying)
                {
                    smoke.Stop();
                }
            }
            return;
        }

        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null)
            {
                if (!smoke.isPlaying)
                {
                    smoke.Play();
                }

                var emission = smoke.emission;
                emission.rateOverTime = Mathf.Lerp(5f, 30f, 1f - healthPercentage);

                var main = smoke.main;
                main.startColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f, 0.5f), Color.black, 1f - healthPercentage);

                main.startLifetime = 10f;
            }
        }
    }
}
