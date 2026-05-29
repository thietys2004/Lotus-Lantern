namespace Game.Core.Services
{
    /// <summary>
    /// Concrete implementation of ILevelService.
    /// Wraps LevelLoader functionality and provides level management through the service locator.
    /// </summary>
    public class LevelService : ILevelService
    {
        private readonly LevelLoader levelLoader;
        private string currentLevelName = "";

        public string CurrentLevelName => currentLevelName;

        public event System.Action<string> OnLevelLoaded;
        public event System.Action OnLevelCleared;

        public LevelService(LevelLoader loader)
        {
            levelLoader = loader;
        }

        public void LoadLevel(string levelName)
        {
            if (levelLoader == null)
            {
                UnityEngine.Debug.LogError("LevelLoader reference is null in LevelService!");
                return;
            }

            currentLevelName = levelName;
            levelLoader.LoadLevel(levelName);
            OnLevelLoaded?.Invoke(levelName);
        }

        public void ClearLevel()
        {
            if (levelLoader == null)
            {
                UnityEngine.Debug.LogError("LevelLoader reference is null in LevelService!");
                return;
            }

            levelLoader.ClearCurrentLevel();
            currentLevelName = "";
            OnLevelCleared?.Invoke();
        }
    }
}
