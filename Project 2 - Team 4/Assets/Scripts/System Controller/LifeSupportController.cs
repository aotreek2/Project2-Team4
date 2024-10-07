using UnityEngine;
using System.Collections;

public class LifeSupportController : MonoBehaviour
{
    public float lifeSupportHealth = 100f;
    public float lifeSupportMaxHealth = 100f;
    public float oxygenLevel = 100f; // Current oxygen level
    public float oxygenDepletionRate = 1f; // Default oxygen depletion rate

    private ShipController shipController; // Reference to ShipController to access generator health
    private bool isUsingOxygenReserve = false; // Whether oxygen reserve is active
    public float oxygenReserve = 50f; // Emergency oxygen reserve

    void Start()
    {
        // Find and reference the ShipController to access generator health
        shipController = FindObjectOfType<ShipController>();

        if (shipController == null)
        {
            Debug.LogError("ShipController not found in the scene.");
        }
    }

    void Update()
    {
        UpdateOxygen();
    }

    void UpdateOxygen()
    {
        Debug.Log("Life Support Health: " + lifeSupportHealth + " / " + lifeSupportMaxHealth);
        Debug.Log("Generator Health: " + shipController.generatorHealth + " / " + shipController.generatorMaxHealth);

        // Only deplete oxygen if life support or generator is damaged
        if (IsLifeSupportOrGeneratorDamaged())
        {
            Debug.Log("Oxygen is depleting because a system is damaged.");
            
            float damagePercentage = 1 - (lifeSupportHealth / lifeSupportMaxHealth);
            float depletionRate = oxygenDepletionRate * damagePercentage;
            oxygenLevel -= depletionRate * Time.deltaTime;
            oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);

            Debug.Log("Current Oxygen Level: " + oxygenLevel);
            Debug.Log("Oxygen Reserve: " + oxygenReserve);

            // Trigger oxygen reserve if oxygen falls below 10%
            if (oxygenLevel < 10f && oxygenReserve > 0 && !isUsingOxygenReserve)
            {
                isUsingOxygenReserve = true;
                Debug.Log("Oxygen Reserve activated!");
            }

            if (isUsingOxygenReserve)
            {
                oxygenReserve -= depletionRate * Time.deltaTime;
                if (oxygenReserve <= 0)
                {
                    oxygenReserve = 0;
                    isUsingOxygenReserve = false;
                    Debug.Log("Oxygen Reserve depleted!");
                }
            }
        }
        else
        {
            Debug.Log("Oxygen is not depleting because both life support and generator are fully functional.");
        }
    }

    bool IsLifeSupportOrGeneratorDamaged()
    {
        bool lifeSupportDamaged = lifeSupportHealth < lifeSupportMaxHealth;
        bool generatorDamaged = shipController.generatorHealth < shipController.generatorMaxHealth;

        Debug.Log("Is life support damaged? " + lifeSupportDamaged);
        Debug.Log("Is generator damaged? " + generatorDamaged);

        return lifeSupportDamaged || generatorDamaged;
    }

    public void DamageLifeSupport(float damage)
    {
        lifeSupportHealth -= damage;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log("Life Support damaged by " + damage + " points.");
    }

    public void RepairLifeSupport(float amount)
    {
        lifeSupportHealth += amount;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log("Life Support repaired by " + amount + " points.");
    }

    public void ReduceLifeSupportEfficiency(float percentage)
    {
        float reduction = lifeSupportMaxHealth * (percentage / 100f);
        lifeSupportHealth -= reduction;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log($"Life Support efficiency reduced by {percentage}%.");
    }
}
