using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI;

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void GoToOptions()
    {
       pauseMenuUI.SetActive(false);
       isPaused = false;
       Time.timeScale =1f;
       SceneManager.LoadScene("Options");
    }

    public void GoToModeSelect()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Mode Select");
    }

   public void GoToCharacterSelect()
{
    pauseMenuUI.SetActive(false);
    isPaused = false;
    Time.timeScale = 1f;
    SceneManager.LoadScene("Player Select");
}

   public void QuitToTitle()
{
    pauseMenuUI.SetActive(false);
    isPaused = false;
    Time.timeScale = 1f;
    SceneManager.LoadScene("Title Screen");
}

    //
    void OnEnable()
{
    UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
}

void OnDisable()
{
    UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
}

void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
{
    string sceneName = scene.name;
    if (sceneName == "Title Screen" || sceneName == "Mode Select" || sceneName == "Player Select")
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }
}

}
