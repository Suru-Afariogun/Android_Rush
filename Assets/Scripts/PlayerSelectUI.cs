using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed for Button

public class PlayerSelectUI : MonoBehaviour
{
    [Header("UI References")]
    public Button confirmButton; // assign in Inspector

    private void Start()
    {
        // Disable the confirm button at the start
        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    public void OnCharacterSelected(string characterName)
    {
        GameManager.Instance.selectedCharacter = characterName;
        Debug.Log("Selected: " + characterName);

        // Enable the confirm button when a character is picked
        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    public void OnConfirmSelection()
    {
        // Safety check — in case no character was selected
        if (string.IsNullOrEmpty(GameManager.Instance.selectedCharacter))
        {
            Debug.LogWarning("No character selected! Cannot proceed.");
            return;
        }

        // Load the correct scene based on the mode chosen
        switch (GameManager.Instance.selectedMode)
        {
            case "Boss":
                SceneManager.LoadScene("000 Bossfight");
                break;
            case "PVP":
                SceneManager.LoadScene("PVP Stage");
                break;
            case "Tag":
                SceneManager.LoadScene("Tag Stage");
                break;
            case "Race":
                SceneManager.LoadScene("Race Stage");
                break;
            default:
                Debug.LogError("Invalid mode or scene not set.");
                break;
        }
    }
}
