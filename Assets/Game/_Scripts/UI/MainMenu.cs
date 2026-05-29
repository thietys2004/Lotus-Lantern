using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu controller.
/// Handles navigation between main menu and level selection/gameplay.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private string levelSelectSceneName = "Level Select";
    [SerializeField] private string gameplaySceneName = "Game play";

    /// <summary>
    /// Start new game - go to level selection.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("<color=yellow>[MainMenu] ► StartGame clicked - Loading Level Select scene...</color>");
        SceneManager.LoadScene(levelSelectSceneName);
    }

    /// <summary>
    /// Continue last game.
    /// </summary>
    public void ContinueGame()
    {
        int lastLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        int lastIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 1);

        Debug.Log($"<color=yellow>[MainMenu] ► ContinueGame clicked - Loading level {lastLevel}</color>");

        PlayerPrefs.SetInt("SelectedLevelIndex", lastIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Open settings menu.
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("<color=yellow>[MainMenu] ► OpenSettings clicked</color>");
        // TODO: Implement settings UI
    }

    /// <summary>
    /// Show credits.
    /// </summary>
    public void ShowCredits()
    {
        Debug.Log("<color=yellow>[MainMenu] ► ShowCredits clicked</color>");
        // TODO: Implement credits UI
    }

    /// <summary>
    /// Quit game.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("<color=yellow>[MainMenu] ► QuitGame clicked</color>");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Check if continue is available.
    /// </summary>
    public bool IsContinueAvailable()
    {
        return PlayerPrefs.HasKey("CurrentLevel");
    }
}
