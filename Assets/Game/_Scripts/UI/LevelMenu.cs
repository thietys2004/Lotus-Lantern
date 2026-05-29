using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

namespace Game.UI
{
    public class LevelMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject levelSlotPrefab;
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject[] pageFrames;
        [SerializeField] private GameObject[] pageFills;

        [Header("Settings")]
        [SerializeField] private int levelsPerPage = 8;
        [SerializeField] private string levelPrefix = "Map_";
        [SerializeField] private string levelsResourcePath = "Levels/";
        [SerializeField] private string gameplaySceneName = "Game play";

        private int currentPage = 1;
        private int maxPage;
        private int totalAvailableLevels = 0;
        private int unlockedLevel = 1;
        private int currentPlayingLevel = 1;

        private List<string> availableLevels = new List<string>();

        private void Start()
        {
            DiscoverAvailableLevels();

            unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            currentPlayingLevel = PlayerPrefs.GetInt("SelectedLevelIndex", 1); // Đọc level hiện tại
            //unlockedLevel = 999; // Dành cho testing

            maxPage = Mathf.CeilToInt((float)totalAvailableLevels / levelsPerPage);

            // 💡 TỰ ĐỘNG NHẢY ĐẾN TRANG CÓ LEVEL ĐANG CHƠI
            currentPage = Mathf.CeilToInt((float)currentPlayingLevel / levelsPerPage);
            if (currentPage < 1) currentPage = 1;
            if (currentPage > maxPage) currentPage = maxPage;

            if (gridParent == null) gridParent = transform;

            RefreshUI();
        }

        private void DiscoverAvailableLevels()
        {
            TextAsset[] levelAssets = Resources.LoadAll<TextAsset>(levelsResourcePath);
            foreach (TextAsset asset in levelAssets)
            {
                if (asset.name.StartsWith(levelPrefix))
                    availableLevels.Add(asset.name);
            }
            availableLevels.Sort();
            totalAvailableLevels = availableLevels.Count;
        }

        public void RefreshUI()
        {
            foreach (Transform child in gridParent) Destroy(child.gameObject);

            int startIndex = (currentPage - 1) * levelsPerPage;

            for (int i = 0; i < levelsPerPage; i++)
            {
                int levelIndex = startIndex + i;
                GameObject newButton = Instantiate(levelSlotPrefab, gridParent);
                TextMeshProUGUI txt = newButton.GetComponentInChildren<TextMeshProUGUI>();
                Button btn = newButton.GetComponent<Button>();
                Image frameImage = newButton.GetComponent<Image>();

                if (levelIndex < totalAvailableLevels)
                {
                    string levelName = availableLevels[levelIndex];
                    int displayNumber = levelIndex + 1;

                    if (txt != null) txt.text = displayNumber.ToString();


                    if (displayNumber == currentPlayingLevel)
                    {
                        btn.interactable = true;
                        if (txt != null) txt.color = Color.yellow;
                        frameImage.color = Color.yellow;
                        newButton.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
                    }
                    else if (displayNumber <= unlockedLevel)
                    {

                        btn.interactable = true;
                        if (txt != null) txt.color = Color.cyan;
                        frameImage.color = Color.white;
                        newButton.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        // LEVEL BỊ KHÓA
                        btn.interactable = false;
                        if (txt != null) txt.color = new Color(1f, 1f, 1f, 0.3f);
                        frameImage.color = Color.white;
                        newButton.transform.localScale = Vector3.one;
                    }

                    if (btn.interactable)
                    {
                        string capturedLevelName = levelName;
                        btn.onClick.AddListener(() => SelectLevel(capturedLevelName, displayNumber));
                    }
                }
                else
                {
                    if (txt != null) txt.text = "";
                    btn.interactable = false;
                    frameImage.color = new Color(1f, 1f, 1f, 0.2f);
                }
            }
            UpdatePageIndicators();
        }

        private void UpdatePageIndicators()
        {
            for (int i = 0; i < pageFrames.Length; i++)
            {
                if (i >= maxPage) { pageFrames[i].SetActive(false); continue; }
                pageFrames[i].SetActive(true);
                if (pageFills[i] != null) pageFills[i].SetActive(i + 1 == currentPage);
            }
        }

        private void SelectLevel(string levelName, int levelNumber)
        {
            PlayerPrefs.SetInt("SelectedLevelIndex", levelNumber);
            PlayerPrefs.SetString("SelectedLevelName", levelName);
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void NextPage() { if (currentPage < maxPage) { currentPage++; RefreshUI(); } }
        public void PreviousPage() { if (currentPage > 1) { currentPage--; RefreshUI(); } }
        public void ReturnToMainMenu() { SceneManager.LoadScene("Main Menu"); }
        public void JumpToPage(int pageNumber) { if (pageNumber >= 1 && pageNumber <= maxPage) { currentPage = pageNumber; RefreshUI(); } }
    }
}