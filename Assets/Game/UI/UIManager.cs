using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {
        public TextMeshProUGUI infoText;
        public int mapmNumber = 1;
        public int stepCount = 0;
        private float playTime = 0f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    }
}

