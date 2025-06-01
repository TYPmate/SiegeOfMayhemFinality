using UnityEngine;
using UnityEngine.AI;

public class EnemyTrigger : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    private bool hasSpawned = false;

    void OnTriggerEnter(Collider other)
    {
        if (!hasSpawned && other.CompareTag("Player"))
        {
            foreach (Transform point in spawnPoints)
            {
                NavMeshHit hit;

                // Sample nearest NavMesh position within 5 units
                if (NavMesh.SamplePosition(point.position, out hit, 5.0f, NavMesh.AllAreas))
                {
                    // Optional: align rotation with the NavMesh normal
                    Quaternion navRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * point.rotation;

                    GameObject enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
                    NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

                    if (agent != null && agent.Warp(hit.position))
                    {
                        enemy.transform.rotation = navRot;
                    }
                    else
                    {
                        Debug.LogError("Failed to warp enemy to NavMesh at: " + hit.position);
                    }

                    // Optional sanity check
                    if (!enemy.GetComponent<NavMeshAgent>().isOnNavMesh)
                    {
                        Debug.LogError("Spawned enemy is NOT on the NavMesh at: " + hit.position);
                    }
                }
                else
                {
                    Debug.LogError("NavMesh.SamplePosition failed at point: " + point.position);
                }
            }
            hasSpawned = true;
        }
    }
}