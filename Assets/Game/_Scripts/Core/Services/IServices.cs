namespace Game.Core.Services
{
    /// <summary>
    /// Interface for audio management services.
    /// Replaces direct AudioManager singleton dependency.
    /// </summary>
    public interface IAudioService
    {
        void PlaySound(string clipName);
        void SetWalkingSound(bool isWalking);
        void PlayFireSound();
        void PlayPickupSound();
        void PlayDoorSound();
        void PlayLotusSound();
        void PlayDeathScream();
        void SetBGMVolume(float volume);
        void SetSFXVolume(float volume);
        float GetBGMVolume();
        float GetSFXVolume();
    }

    /// <summary>
    /// Interface for game state management.
    /// Handles game progression and level information.
    /// </summary>
    public interface IGameStateService
    {
        void StartLevel(int levelIndex);
        void NextLevel();
        void EndGame(bool isWin);
        void Restart();

        int CurrentLevel { get; }
        int MaxLevels { get; }
        bool IsGameEnded { get; }
        bool IsGamePaused { get; }

        event System.Action<int> OnLevelStarted;
        event System.Action OnLevelCompleted;
        event System.Action<bool> OnGameEnded;
    }

    /// <summary>
    /// Interface for UI management and updates.
    /// Separates UI logic from game state.
    /// </summary>
    public interface IUIService
    {
        void UpdateGameplayUI(int stepCount, float playTime);
        void UpdateLanternBar(int currentLamps, int maxLamps);
        void UpdateInventoryUI(int lotusCount, int lighterCount, int keyCount);
        void UpdateLotusCount(int count);
        void AddStep();
        void ShowEndGamePanel(bool isWin, int stepCount = 0, float playTime = 0);
        void ShowPausePanel();
        void HidePausePanel();
        void SetLevelDisplayName(string levelName);

        event System.Action OnPauseRequested;
        event System.Action OnResumeRequested;
    }

    /// <summary>
    /// Interface for character interactions.
    /// Allows decoupled communication between game objects.
    /// </summary>
    public interface ICharacter
    {
        bool IsSafePath();
        void TakeDamage();
        void Heal();
        int HealthPoints { get; }
    }

    /// <summary>
    /// Interface for level loading and management.
    /// </summary>
    public interface ILevelService
    {
        void LoadLevel(string levelName);
        void ClearLevel();
        string CurrentLevelName { get; }

        event System.Action<string> OnLevelLoaded;
        event System.Action OnLevelCleared;
    }

    /// <summary>
    /// Interface for input handling.
    /// Allows abstraction from specific input systems (Keyboard, Mobile, Gamepad).
    /// </summary>
    public interface IInputService
    {
        UnityEngine.Vector2 GetMovementInput();
        bool GetPlaceLanternInput();
        bool GetInteractInput();
        bool GetPauseInput();
    }

    /// <summary>
    /// Interface for soul fire (lantern) management.
    /// Manages all lanterns and their state in the level.
    /// </summary>
    public interface ISoulFireService
    {
        void ClearAllLamps();
        void RegisterLamp(UnityEngine.GameObject lamp);
        void UnregisterLamp(UnityEngine.GameObject lamp);
        int GetActiveLampCount();

        event System.Action OnAllLampsCleared;
    }
}
