using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
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

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }
        void Start()
        {
            if (string.IsNullOrEmpty(levelDisplayName))
            {
                levelDisplayName = SceneManager.GetActiveScene().name;
            }
            Time.timeScale = 1f;
            AudioListener.pause = false;
            UpdateInfoBoard();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isGameEnded)
            {
                playTime += Time.deltaTime;
                UpdateInfoBoard();
            }
        }
        public void UpdateInfoBoard()
        {
            if (infoText == null) return;

            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);

            infoText.text = $"LEVEL: {levelDisplayName}\nSTEPS: {stepCount:000}\nTIMER: {minutes:00}:{seconds:00}";
        }
        public void ShowEndGamePanel(bool isWin)
        {
            isGameEnded = true;
            endGamePanel.SetActive(true);

            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (btnNextLevel != null)
            {
                btnNextLevel.SetActive(isWin);
            }
            if (isWin)
            {
                txtEndGameTitle.text = "YOU WIN!";
                txtEndGameTitle.color = Color.cyan;
            }
            else
            {
                txtEndGameTitle.text = "YOU LOST!";
                txtEndGameTitle.color = Color.red;
                txtEndGameTitle.fontSize = 60;
                txtEndGameTitle.fontStyle = FontStyles.Bold;
            }


            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);

            txtEndGameStats.text = $"LEVEL: {levelDisplayName}\nSTEPS: {stepCount:000}\nTIME: {minutes:00}:{seconds:00}";
        }
        public void RestartLevel()
        {

            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void LoadNextLevel()
        {

            Time.timeScale = 1f;


            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        public void GoToHome()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Main Menu");
        }
        public void AddStep()
        {
            stepCount++;
            UpdateInfoBoard();
        }
        public void ResetSteps()
        {
            stepCount = 0;
            playTime = 0f;
            UpdateInfoBoard();
        }

        public void PauseGame()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            AudioListener.pause = true;

            if (bgmSlider != null && Game.Core.AudioManager.Instance != null)
                bgmSlider.value = Game.Core.AudioManager.Instance.bgmVolume;

            if (sfxSlider != null && Game.Core.AudioManager.Instance != null)
                sfxSlider.value = Game.Core.AudioManager.Instance.sfxVolume;

            UpdateAudioUI();

        }

        public void TogglePause()
        {

            if (pausePanel.activeInHierarchy)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        public void ResumeGame()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
        public void OnBGMVolumeChanged(float value)
        {
            if (Game.Core.AudioManager.Instance != null)
            {
                Game.Core.AudioManager.Instance.SetBGMVolume(value);
                UpdateAudioUI();
            }
        }
        public void OnSFXVolumeChanged(float value)
        {
            if (Game.Core.AudioManager.Instance != null)
            {
                Game.Core.AudioManager.Instance.SetSFXVolume(value);
                UpdateAudioUI();
            }
        }
        private void UpdateAudioUI()
        {
            if (txtBgmPercent != null && bgmSlider != null)
                txtBgmPercent.text = Mathf.RoundToInt(bgmSlider.value * 100) + "%";

            if (txtSfxPercent != null && sfxSlider != null)
                txtSfxPercent.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
        }
        public void UpdateLanternBar(int currentLamps, int maxLamps)
        {
            if (lanternImageUI == null || lanternSprites.Length == 0 || maxLamps <= 0) return;

            float fillRatio = (float)currentLamps / maxLamps;

            int index = Mathf.RoundToInt(fillRatio * (lanternSprites.Length - 1));

            index = Mathf.Clamp(index, 0, lanternSprites.Length - 1);

            lanternImageUI.sprite = lanternSprites[index];

            if (txtMaxFire != null)
            {
                txtMaxFire.text = currentLamps + "/" + maxLamps;
            }

        }
        public void UpdateLotusCount(int currentCount)
        {
            if (txtLotusCount != null)
            {
                txtLotusCount.text = currentCount.ToString();
            }
        }
        public void UpdateLightCount(int currentCount)
        {
            if (txtLightCount != null)
            {
                txtLightCount.text = currentCount.ToString();
            }
        }
        public void UpdateKeyCount(int currentCount)
        {
            if (txtKeyCount != null)
            {
                txtKeyCount.text = currentCount.ToString();
            }
        }
    }
}

