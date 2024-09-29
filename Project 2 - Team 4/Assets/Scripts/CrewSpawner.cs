using UnityEngine;

public class CrewSpawner : MonoBehaviour
{
    public GameObject workerPrefab;
    public GameObject engineerPrefab;
    public Transform spawnPoint;

    public void SpawnWorker()
    {
        Instantiate(workerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    public void SpawnEngineer()
    {
        Instantiate(engineerPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
