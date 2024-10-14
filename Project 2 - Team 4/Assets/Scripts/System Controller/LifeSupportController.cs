using UnityEngine;
using System.Collections;

public class LifeSupportController : MonoBehaviour
{
    public float lifeSupportHealth = 100f;
    public float lifeSupportMaxHealth = 100f;

    public ParticleSystem lifeSupportFireParticles;
    public ParticleSystem[] lifeSupportSmokeParticlesArray;
    public bool isLifeSupportOnFire = false;

    private bool isTakingDamageOverTime = false;
    private float damageOverTimeRate = 0f;

    private ShipController shipController;

    public float LifeSupportEfficiency
    {
        get
        {
            return lifeSupportHealth / lifeSupportMaxHealth;
        }
    }

    void Start()
    {
        shipController = FindObjectOfType<ShipController>();

        if (lifeSupportFireParticles != null)
        {
            lifeSupportFireParticles.Stop();
        }

        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
        {
            if (smoke != null)
            {
                smoke.Stop();
            }
        }
    }

    void Update()
    {
        if (isLifeSupportOnFire)
        {
            UpdateFire();
        }

        if (isTakingDamageOverTime)
        {
            float damage = damageOverTimeRate * Time.deltaTime;
            DamageLifeSupport(damage);

            if (lifeSupportHealth <= 0f)
            {
                isTakingDamageOverTime = false;
            }
        }
    }

    public void DamageLifeSupport(float damage)
    {
        lifeSupportHealth -= damage;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);

        if (lifeSupportHealth <= lifeSupportMaxHealth * 0.5f && !isLifeSupportOnFire)
        {
            StartLifeSupportFire();
        }

        UpdateSmokeEffects();
    }

    public void RepairLifeSupport(float amount)
    {
        lifeSupportHealth += amount;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);

        if (lifeSupportHealth >= lifeSupportMaxHealth)
        {
            StopLifeSupportFire();
        }

        UpdateSmokeEffects();
    }

    public void StartLifeSupportFire()
    {
        if (lifeSupportFireParticles != null)
        {
            lifeSupportFireParticles.Play();
            isLifeSupportOnFire = true;
        }

        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
        {
            if (smoke != null && !smoke.isPlaying)
            {
                smoke.Play();
            }
        }
    }

    public void StopLifeSupportFire()
    {
        if (lifeSupportFireParticles != null)
        {
            lifeSupportFireParticles.Stop();
            isLifeSupportOnFire = false;
        }

        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
        {
            if (smoke != null && smoke.isPlaying)
            {
                smoke.Stop();
            }
        }
    }

    private void UpdateFire()
    {
        float fireDamage = Time.deltaTime * 5f;
        DamageLifeSupport(fireDamage);

        if (lifeSupportFireParticles != null)
        {
            float healthPercentage = lifeSupportHealth / lifeSupportMaxHealth;
            var emission = lifeSupportFireParticles.emission;
            emission.rateOverTime = Mathf.Lerp(10f, 50f, 1f - healthPercentage);
        }
    }

    private void UpdateSmokeEffects()
    {
        if (lifeSupportSmokeParticlesArray.Length == 0) return;

        float healthPercentage = lifeSupportHealth / lifeSupportMaxHealth;

        bool lifeSupportFullyRepaired = Mathf.Approximately(healthPercentage, 1f);

        if (lifeSupportFullyRepaired)
        {
            foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
            {
                if (smoke != null && smoke.isPlaying)
                {
                    smoke.Stop();
                }
            }
            return;
        }

        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
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

    public void ReduceLifeSupportEfficiency(float percentage)
    {
        float reductionAmount = lifeSupportMaxHealth * (percentage / 100f);
        DamageLifeSupport(reductionAmount);
    }
}
