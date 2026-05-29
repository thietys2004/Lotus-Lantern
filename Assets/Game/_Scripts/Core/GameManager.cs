using Game.Core.Services;
using Game.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    /// <summary>
    /// Main game manager that orchestrates game flow and level progression.
    /// Handles scene transitions, level loading, and game state management.
    /// Integrates with ServiceLocator for dependency injection.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxLevels = 50;
        [SerializeField] private string levelNamePrefix = "Map_";

        [Header("Dependencies")]
        [SerializeField] private LevelLoader levelLoader;

        private GameConfig config;
        private int currentLevelIndex = 1;

        // Progression tracking
        private int totalLevelsCompleted = 0;

        public static GameManager Instance { get; private set; }

        // Events
        public event System.Action<int> OnLevelStarted;
        public event System.Action OnLevelCompleted;
        public event System.Action<bool> OnGameEnded;

        public int CurrentLevelIndex => currentLevelIndex;
        public int MaxLevels => maxLevels;
        public int TotalLevelsCompleted => totalLevelsCompleted;

        private void Awake()
        {
            config = GameConfig.Instance;

            // Singleton pattern with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Register services
                ServiceLocator.Instance.Register<IGameStateService>(
                    new GameStateService(this)
                );
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Try to load from PlayerPrefs for resuming
            int savedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            totalLevelsCompleted = PlayerPrefs.GetInt("LevelsCompleted", 0);

            // Load the level
            StartLevel(savedLevel);
        }

        /// <summary>
        /// Start a specific level by index.
        /// </summary>
        public void StartLevel(int levelIndex)
        {
            if (levelIndex < 1 || levelIndex > maxLevels)
            {
                Debug.LogError($"Invalid level index: {levelIndex}. Max: {maxLevels}");
                return;
            }

            currentLevelIndex = levelIndex;

            // Save current level
            PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex);
            PlayerPrefs.Save();

            string levelName = levelNamePrefix + levelIndex.ToString("D2");
            Debug.Log($"<color=cyan>━━━━ LOADING LEVEL {levelIndex}: {levelName} ━━━━</color>");

            if (levelLoader != null)
            {
                levelLoader.LoadLevel(levelName);
            }
            else
            {
                Debug.LogError("LevelLoader not assigned!");
            }

            OnLevelStarted?.Invoke(levelIndex);
        }

        /// <summary>
        /// Load the next level in sequence.
        /// </summary>
        public void NextLevel()
        {
            totalLevelsCompleted++;
            PlayerPrefs.SetInt("LevelsCompleted", totalLevelsCompleted);
            PlayerPrefs.Save();

            if (currentLevelIndex < maxLevels)
            {
                StartLevel(currentLevelIndex + 1);
                OnLevelCompleted?.Invoke();
            }
            else
            {
                Debug.Log($"<color=yellow>✓ CHÚC MỪNG! Bạn đã hoàn thành tất cả {maxLevels} level!</color>");
                EndGame(true);
            }
        }

        /// <summary>
        /// Restart current level.
        /// </summary>
        public void RestartLevel()
        {
            Debug.Log($"Restarting level {currentLevelIndex}");
            StartLevel(currentLevelIndex);
        }

        /// <summary>
        /// Load a specific level by name (for menu).
        /// </summary>
        public void LoadLevelByName(string levelName)
        {
            // Extract index from level name (e.g., "Map_01" -> 1)
            string indexStr = levelName.Replace(levelNamePrefix, "");
            if (int.TryParse(indexStr, out int levelIndex))
            {
                StartLevel(levelIndex);
            }
            else
            {
                Debug.LogError($"Cannot parse level index from name: {levelName}");
            }
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        public void PlayerDied()
        {
            Debug.Log("Player died. Restarting level...");
            Invoke(nameof(RestartLevel), 1f);
        }

        /// <summary>
        /// End game (win or loss).
        /// </summary>
        public void EndGame(bool isWin)
        {
            if (isWin)
            {
                Debug.Log("<color=green>GAME WON!</color>");
            }
            else
            {
                Debug.Log("<color=red>GAME OVER!</color>");
            }

            OnGameEnded?.Invoke(isWin);

            // Pause game
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Resume game after pause.
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Return to main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            levelLoader.ClearCurrentLevel();
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Save level progress.
        /// </summary>
        public void SaveProgress()
        {
            PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex);
            PlayerPrefs.SetInt("LevelsCompleted", totalLevelsCompleted);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Get level stats.
        /// </summary>
        public (int current, int max, int completed) GetLevelStats()
        {
            return (currentLevelIndex, maxLevels, totalLevelsCompleted);
        }

        private void OnDestroy()
        {
            SaveProgress();
        }
    }

    /// <summary>
    /// Game state service implementation for ServiceLocator.
    /// </summary>
    public class GameStateService : Game.Core.Services.IGameStateService
    {
        private GameManager gameManager;

        public int CurrentLevel => gameManager.CurrentLevelIndex;
        public int MaxLevels => gameManager.MaxLevels;
        public bool IsGameEnded { get; private set; }
        public bool IsGamePaused { get; private set; }

        public event System.Action<int> OnLevelStarted;
        public event System.Action OnLevelCompleted;
        public event System.Action<bool> OnGameEnded;

        public GameStateService(GameManager manager)
        {
            gameManager = manager;
            gameManager.OnLevelStarted += (level) => OnLevelStarted?.Invoke(level);
            gameManager.OnLevelCompleted += () => OnLevelCompleted?.Invoke();
            gameManager.OnGameEnded += (isWin) => { IsGameEnded = true; OnGameEnded?.Invoke(isWin); };
        }

        public void StartLevel(int levelIndex)
        {
            gameManager.StartLevel(levelIndex);
        }

        public void NextLevel()
        {
            gameManager.NextLevel();
        }

        public void EndGame(bool isWin)
        {
            gameManager.EndGame(isWin);
        }

        public void Restart()
        {
            gameManager.RestartLevel();
        }
    }
}
