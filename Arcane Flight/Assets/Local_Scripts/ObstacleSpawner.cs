using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject[] buildingPrefabs;

    public float spawnDistance = 50f;   // how far ahead of player
    public float spawnGap = 15f;        // distance between buildings (Z)

    public float leftLaneX = -10f;
    public float rightLaneX = 10f;
    public float groundY = 0f;

    float nextSpawnZ;
    bool spawnLeft = true;

    void Start()
    {
        if (player != null)
            nextSpawnZ = player.position.z + spawnDistance;
    }

    void Update()
    {
        if (!player || buildingPrefabs.Length == 0)
            return;

        while (player.position.z + spawnDistance > nextSpawnZ)
        {
            SpawnBuilding();
            nextSpawnZ += spawnGap;
        }
    }

    void SpawnBuilding()
    {
        float xPos = spawnLeft ? leftLaneX : rightLaneX;
        spawnLeft = !spawnLeft; // alternate lanes

        GameObject prefab =
            buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];

        Vector3 spawnPos = new Vector3(xPos, groundY, nextSpawnZ);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
