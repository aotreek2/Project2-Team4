using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] float spawnRate = 0.5f;
    [SerializeField] GameObject[] asteroids;


    List<GameObject> allAsteroids = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnAsteroid", 0f, spawnRate);
    }

    void SpawnAsteroid()
    {
        int randomAsteroid = Random.Range(0, 3);
        GameObject spawnedAsteroid = Instantiate(asteroids[randomAsteroid], new Vector3(180f, Random.Range(0f, -40f), Random.Range(-150f, 150f)), Quaternion.identity);
        allAsteroids.Add(spawnedAsteroid);
    }
}
