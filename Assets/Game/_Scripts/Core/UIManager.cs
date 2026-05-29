using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Core.Services;

namespace Game.UI
{
    public class UIManager : MonoBehaviour, IUIService
    {
        public TextMeshProUGUI infoText;
        public string levelDisplayName;
        public int stepCount = 0;
        private float playTime = 0f;

        public static UIManager Instance { get; private set; }
        public Image lanternImageUI;
        public Sprite[] lanternSprites;
        public TextMeshProUGUI txtMaxFire;
        public TextMeshProUGUI txtLotusCount;
        public TextMeshProUGUI txtLightCount;
        public TextMeshProUGUI txtKeyCount;

        [Header("End Game Panel")]
        public GameObject endGamePanel;
        public TextMeshProUGUI txtEndGameTitle;
        public TextMeshProUGUI txtEndGameStats;
        public GameObject btnNextLevel;

        [Header("Pause Panel")]
        public GameObject pausePanel;
        public Slider bgmSlider;
        public Slider sfxSlider;
        public TextMeshProUGUI txtBgmPercent;
        public TextMeshProUGUI txtSfxPercent;

        private bool isGameEnded = false;

        public event System.Action OnPauseRequested;
        public event System.Action OnResumeRequested;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ServiceLocator.Instance.Register<IUIService>(this);
            }
            else Destroy(this);
        }

        void Start()
        {
            // 💡 CẬP NHẬT: Lấy tên Level và số thứ tự từ PlayerPrefs (do LevelMenu lưu)
            string savedLevelName = PlayerPrefs.GetString("SelectedLevelName", "");
            int savedLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 1);

            if (!string.IsNullOrEmpty(savedLevelName))
            {

                levelDisplayName = $"{savedLevelIndex}";


            }
            else if (string.IsNullOrEmpty(levelDisplayName))
            {

                levelDisplayName = SceneManager.GetActiveScene().name;
            }

            Time.timeScale = 1f;
            AudioListener.pause = false;
            UpdateInfoBoard();
        }

        void Update()
        {
            if (!isGameEnded)
            {
                playTime += Time.deltaTime;
                UpdateInfoBoard();
            }
        }

        public void UpdateGameplayUI(int stepCount, float playTime)
        {
            this.stepCount = stepCount;
            this.playTime = playTime;
            UpdateInfoBoard();
        }

        public void ShowPausePanel() { PauseGame(); OnPauseRequested?.Invoke(); }
        public void HidePausePanel() { ResumeGame(); OnResumeRequested?.Invoke(); }
        public void SetLevelDisplayName(string levelName) { levelDisplayName = levelName; }

        public void UpdateInventoryUI(int lotusCount, int lighterCount, int keyCount)
        {
            UpdateLotusCount(lotusCount);
            UpdateLightCount(lighterCount);
            UpdateKeyCount(keyCount);
        }

        // 💡 CẬP NHẬT: Thêm string customStatus để hiển thị lý do chết (nếu có)
        void IUIService.ShowEndGamePanel(bool isWin, int stepCount, float playTime)
        {
            // Chuyển tiếp dữ liệu sang hàm mới bên dưới
            this.ShowEndGamePanel(isWin, stepCount, playTime, "");
        }

        // 💡 CẬP NHẬT: Hàm nâng cấp có thêm customStatus (Giữ nguyên đoạn này)
        public void ShowEndGamePanel(bool isWin, int stepCount = 0, float playTime = 0, string customStatus = "")
        {
            isGameEnded = true;
            endGamePanel.SetActive(true);
            Time.timeScale = 0f;
            AudioListener.pause = true;

            if (btnNextLevel != null) btnNextLevel.SetActive(isWin);

            if (isWin)
            {
                txtEndGameTitle.text = "LEVEL CLEARED!";
                txtEndGameTitle.color = Color.green;

                // Lưu tiến trình mở khóa màn chơi mới
                int currentLevel = PlayerPrefs.GetInt("SelectedLevelIndex", 1);
                int maxUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
                if (currentLevel >= maxUnlocked)
                {
                    PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                // Hiển thị trạng thái/lý do thua nếu có truyền vào
                txtEndGameTitle.text = string.IsNullOrEmpty(customStatus) ? "GAME OVER!" : customStatus;
                txtEndGameTitle.color = Color.red;
                txtEndGameTitle.fontSize = 50; // Chỉnh lại font cho vừa khung
            }
            int finalSteps = stepCount > 0 ? stepCount : this.stepCount;
            float finalTime = playTime > 0 ? playTime : this.playTime;
            int minutes = Mathf.FloorToInt(finalTime / 60f);
            int seconds = Mathf.FloorToInt(finalTime % 60f);
            txtEndGameStats.text = $"LEVEL: {levelDisplayName}\nSTEPS: {finalSteps:000}\nTIME: {minutes:00}:{seconds:00}";
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // 💡 CẬP NHẬT: Load JSON File tiếp theo thay vì Load Scene Index
        public void LoadNextLevel()
        {
            Time.timeScale = 1f;
            int currentLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 1);
            int nextLevelIndex = currentLevelIndex + 1;

            // Tìm file JSON của level tiếp theo
            TextAsset[] levelAssets = Resources.LoadAll<TextAsset>("Levels/");
            System.Collections.Generic.List<string> maps = new System.Collections.Generic.List<string>();
            foreach (var asset in levelAssets)
            {
                if (asset.name.StartsWith("Map_")) maps.Add(asset.name);
            }
            maps.Sort();

            // Nếu còn level tiếp theo thì cập nhật PlayerPrefs
            if (nextLevelIndex <= maps.Count)
            {
                PlayerPrefs.SetInt("SelectedLevelIndex", nextLevelIndex);
                PlayerPrefs.SetString("SelectedLevelName", maps[nextLevelIndex - 1]);
                PlayerPrefs.Save();
            }

            // Tải lại Scene Gameplay hiện tại (Game Manager sẽ tự động đọc file JSON mới)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToHome() { Time.timeScale = 1f; SceneManager.LoadScene("Main Menu"); }
        public void AddStep() { stepCount++; UpdateInfoBoard(); }
        public void ResetSteps() { stepCount = 0; playTime = 0f; UpdateInfoBoard(); }

        public void PauseGame()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (bgmSlider != null && Game.Core.AudioManager.Instance != null) bgmSlider.value = Game.Core.AudioManager.Instance.bgmVolume;
            if (sfxSlider != null && Game.Core.AudioManager.Instance != null) sfxSlider.value = Game.Core.AudioManager.Instance.sfxVolume;
            UpdateAudioUI();
        }

        public void TogglePause() { if (pausePanel.activeInHierarchy) ResumeGame(); else PauseGame(); }

        public void ResumeGame()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        public void OnBGMVolumeChanged(float value) { if (Game.Core.AudioManager.Instance != null) { Game.Core.AudioManager.Instance.SetBGMVolume(value); UpdateAudioUI(); } }
        public void OnSFXVolumeChanged(float value) { if (Game.Core.AudioManager.Instance != null) { Game.Core.AudioManager.Instance.SetSFXVolume(value); UpdateAudioUI(); } }

        private void UpdateAudioUI()
        {
            if (txtBgmPercent != null && bgmSlider != null) txtBgmPercent.text = Mathf.RoundToInt(bgmSlider.value * 100) + "%";
            if (txtSfxPercent != null && sfxSlider != null) txtSfxPercent.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
        }

        public void UpdateInfoBoard()
        {
            if (infoText == null) return;
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            infoText.text = $"LEVEL: {levelDisplayName}\nSTEPS: {stepCount:000}\nTIMER: {minutes:00}:{seconds:00}";
        }

        public void UpdateLanternBar(int currentLamps, int maxLamps)
        {
            if (lanternImageUI == null || lanternSprites.Length == 0 || maxLamps <= 0) return;
            float fillRatio = (float)currentLamps / maxLamps;
            int index = Mathf.Clamp(Mathf.RoundToInt(fillRatio * (lanternSprites.Length - 1)), 0, lanternSprites.Length - 1);
            lanternImageUI.sprite = lanternSprites[index];
            if (txtMaxFire != null) txtMaxFire.text = currentLamps + "/" + maxLamps;
        }

        public void UpdateLotusCount(int currentCount) { if (txtLotusCount != null) txtLotusCount.text = currentCount.ToString(); }
        public void UpdateLightCount(int currentCount) { if (txtLightCount != null) txtLightCount.text = currentCount.ToString(); }
        public void UpdateKeyCount(int currentCount) { if (txtKeyCount != null) txtKeyCount.text = currentCount.ToString(); }
    }
}