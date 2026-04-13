using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {

        public TextMeshProUGUI infoText;
        public int mapmNumber = 1;
        public int stepCount = 0;
        private float playTime = 0f;

        public static UIManager Instance { get; private set; }
        public Image lanternImageUI;
        public Sprite[] lanternSprites;
        public TextMeshProUGUI txtMaxFire;
        public TextMeshProUGUI txtLotusCount;
        public TextMeshProUGUI txtLightCount;
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
            UpdateInfoBoard();
        }

        // Update is called once per frame
        void Update()
        {
            playTime += Time.deltaTime;
            UpdateInfoBoard();
        }
        public void UpdateInfoBoard()
        {
            if (infoText == null) return;

            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);

            infoText.text = $"ROOM: {mapmNumber}\nSTEPS: {stepCount:000}\nTIMER: {minutes:00}:{seconds:00}";
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
    }
}

