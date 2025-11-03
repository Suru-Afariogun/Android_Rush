// using UnityEngine;

// public class PlayerSpawner : MonoBehaviour
// {
//     public Transform spawnPoint;
//     public GameObject playerPrefab;

//     void Start()
//     {
//         Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
//     }
// }

using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform spawnPoint; // assign in Inspector

    void Start()
    {
        GameObject prefab = GameManager.Instance.GetSelectedPlayerPrefab();

        if (prefab != null)
        {
            Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("No player prefab found to spawn!");
        }
    }
}
