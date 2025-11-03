using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame() => SceneManager.LoadScene("Mode Select");
    public void OpenOptions() => SceneManager.LoadScene("Options");
    public void QuitGame() => Application.Quit();
}
