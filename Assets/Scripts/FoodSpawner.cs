using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

    public int foodCount = 20;
    public float spawnRadius = 30f;

    void Start()
    {
        for (int i = 0; i < foodCount; i++)
        {
            Vector3 randomPos = transform.position +
                new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    50f,
                    Random.Range(-spawnRadius, spawnRadius)
                );

            RaycastHit hit;

            if (Physics.Raycast(randomPos, Vector3.down, out hit, 100f))
            {
                Instantiate(
                    foodPrefab,
                    hit.point + Vector3.up*0.7f,
                    Quaternion.identity
                );
            }
        }
    }
}