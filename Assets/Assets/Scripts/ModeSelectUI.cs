using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectUI : MonoBehaviour {
    public void OnPVPSelected() {
        GameManager.Instance.selectedMode = "PVP";
        SceneManager.LoadScene("Player Select");
    }

    public void OnTagSelected() {
        GameManager.Instance.selectedMode = "Tag";
        SceneManager.LoadScene("Player Select");
    }

    public void OnBossSelected() {
        GameManager.Instance.selectedMode = "Boss";
        SceneManager.LoadScene("Player Select");
    }
    public void OnRaceSelected() {
        GameManager.Instance.selectedMode = "Race";
        SceneManager.LoadScene("Player Select");
    }
}
