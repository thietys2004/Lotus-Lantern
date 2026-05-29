using Game.Core.Services;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Handles loading the selected level when the Gameplay scene starts.
    /// </summary>
    public class GameplaySceneLoader : MonoBehaviour
    {
        private ILevelService levelService;

        void Awake()
        {
            Debug.Log("<color=cyan>[GameplaySceneLoader] Awake called - Script is running!</color>");
        }

        void Start()
        {
            Debug.Log("<color=green>[GameplaySceneLoader] ========== LOADING LEVEL ==========</color>");

            // Retrieve the selected level name from PlayerPrefs
            string selectedLevelName = PlayerPrefs.GetString("SelectedLevelName", "");
            int selectedLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);

            Debug.Log($"[GameplaySceneLoader] PlayerPrefs - SelectedLevelName: '{selectedLevelName}', SelectedLevelIndex: {selectedLevelIndex}");

            if (string.IsNullOrEmpty(selectedLevelName))
            {
                Debug.LogError("[GameplaySceneLoader] ✗ No level selected! (SelectedLevelName is empty)");
                // Optionally, load the MainMenu scene if no level is selected
                // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                return;
            }

            // Get the LevelService from the ServiceLocator
            Debug.Log("[GameplaySceneLoader] Getting LevelService from ServiceLocator...");
            levelService = ServiceLocator.Instance.Get<ILevelService>();

            if (levelService == null)
            {
                Debug.LogError("[GameplaySceneLoader] ✗ LevelService not registered in ServiceLocator!");
                return;
            }
            Debug.Log("[GameplaySceneLoader] ✓ LevelService retrieved");

            // Load the selected level
            Debug.Log($"[GameplaySceneLoader] Loading level: {selectedLevelName}");
            levelService.LoadLevel(selectedLevelName);
            Debug.Log($"<color=green>[GameplaySceneLoader] ✓ Successfully loaded level: {selectedLevelName}</color>");
            Debug.Log("<color=green>[GameplaySceneLoader] ========== LEVEL LOADED ==========</color>");
        }
    }
}