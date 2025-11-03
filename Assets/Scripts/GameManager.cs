using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Selection Info")]
    public string selectedMode;
    public string selectedCharacter;
    public GameObject[] playerPrefabs; // assign in Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetSelectedPlayerPrefab()
    {
        foreach (GameObject prefab in playerPrefabs)
        {
            if (prefab.name == selectedCharacter)
                return prefab;
        }
        Debug.LogWarning("No matching prefab found for: " + selectedCharacter);
        return null;
    }
}
