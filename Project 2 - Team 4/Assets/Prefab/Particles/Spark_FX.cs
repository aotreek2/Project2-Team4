using UnityEngine;

public class ElectricSparks : MonoBehaviour
{
    private ParticleSystem sparkParticles;

    void Start()
    {
        // Get the particle system component
        sparkParticles = GetComponent<ParticleSystem>();

        // Configure particle system settings for electric spark effect
        var main = sparkParticles.main;
        main.startSpeed = Random.Range(3f, 5f); // Speed of particles
        main.startLifetime = 0.2f; // How long sparks last
        main.startSize = Random.Range(0.05f, 0.1f); // Size of sparks
        main.startColor = new Color(0.8f, 0.9f, 1.0f); // Light blue-ish electric color

        // Emission settings
        var emission = sparkParticles.emission;
        emission.rateOverTime = 0; // Constant emission turned off
        emission.rateOverDistance = 0;

        // Shape settings
        var shape = sparkParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone; // Sparks fly outward in a cone
        shape.angle = 45f; // Spread angle for the sparks
        shape.radius = 0.1f; // Radius of emission source

        // Lighting effects
        var lights = sparkParticles.lights;
        lights.enabled = true;
        lights.ratio = 0.1f; // Small amount of lights to mimic electric effect
        lights.intensityMultiplier = 1.5f;

        // Make particles flicker
        var noise = sparkParticles.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 10f;

        // Sparks should only occur at random intervals (to simulate a short)
        StartCoroutine(TriggerRandomSparks());
    }

    System.Collections.IEnumerator TriggerRandomSparks()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.3f, 2.0f)); // Random delay between sparks
            sparkParticles.Emit(Random.Range(5, 15)); // Emit a random number of sparks
        }
    }
}
