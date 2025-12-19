// using UnityEngine;

// public class PauseMenuManager : MonoBehaviour
// {
//     public static PauseMenuManager Instance;
//     public GameObject pauseMenuPrefab;
//     private GameObject pauseMenuInstance;

//     void Awake()
//     {
//         // Ensure only one exists
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//         DontDestroyOnLoad(gameObject);

//         // Spawn the Pause Menu once
//         if (pauseMenuPrefab != null && pauseMenuInstance == null)
//         {
//             pauseMenuInstance = Instantiate(pauseMenuPrefab);
//             pauseMenuInstance.SetActive(false); // Start disabled
//             DontDestroyOnLoad(pauseMenuInstance);
//         }
//     }

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             TogglePause();
//         }

//         // Optional controller support
//         if (Input.GetButtonDown("Start") || Input.GetButtonDown("Pause"))
//         {
//             TogglePause();
//         }
//     }

//     public void TogglePause()
//     {
//         if (pauseMenuInstance == null) return;

//         bool currentlyActive = pauseMenuInstance.activeSelf;
//         pauseMenuInstance.SetActive(!currentlyActive);

//         Time.timeScale = currentlyActive ? 1f : 0f;
//         PauseMenu.isPaused = !currentlyActive;
//     }
// }



using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;
    public GameObject pauseMenuPrefab;
    private GameObject pauseMenuInstance;

      private GlobalControls.cs.GlobalControls controls;
      private PauseMenu pauseMenu;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new GlobalControls.cs.GlobalControls();
        controls.UI.Pause.performed += ctx => TogglePause();

        if (pauseMenuPrefab != null && pauseMenuInstance == null)
        {
            pauseMenuInstance = Instantiate(pauseMenuPrefab);
            pauseMenu = pauseMenuInstance.GetComponent<PauseMenu>();
            pauseMenuInstance.SetActive(false);
            DontDestroyOnLoad(pauseMenuInstance);
        }
    }

    void OnEnable() => controls.UI.Enable();
    void OnDisable() => controls.UI.Disable();

    public void TogglePause()
    {
        if (pauseMenuInstance == null || pauseMenu == null) return;

        if (PauseMenu.isPaused)
        {
            pauseMenu.Resume();
            pauseMenuInstance.SetActive(false);
        }
        else
        {
            pauseMenuInstance.SetActive(true);
            pauseMenu.Pause();
        }
}
}