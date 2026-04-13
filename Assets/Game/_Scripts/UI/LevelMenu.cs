using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class LevelMenu : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject levelSlotPrefab;
        public Transform gridParent;

        [Header("Level Settings")]
        public int totalLevels = 15;
        public int levelsPerPage = 8;
        private int currentPage = 1;
        private int maxPage;
        public GameObject[] pageFrames;
        public GameObject[] pageFills;


        void Start()
        {

            maxPage = Mathf.CeilToInt((float)totalLevels / levelsPerPage);

            RefreshUI();
        }

        public void RefreshUI()
        {

            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }

            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);


            int startLevel = (currentPage - 1) * levelsPerPage + 1;


            for (int i = 0; i < levelsPerPage; i++)
            {
                int levelNum = startLevel + i;
                GameObject newButton = Instantiate(levelSlotPrefab, gridParent);

                TextMeshProUGUI txt = newButton.GetComponentInChildren<TextMeshProUGUI>();
                Button btn = newButton.GetComponent<Button>();
                Image frameImage = newButton.GetComponent<Image>();
                if (levelNum <= totalLevels)
                {
                    if (txt != null) txt.text = levelNum.ToString();

                    if (levelNum <= unlockedLevel)
                    {

                        btn.interactable = true;
                        txt.color = new Color(0f, 1f, 1f, 1f);
                        frameImage.color = new Color(1f, 1f, 1f, 1f);

                        int capturedNum = levelNum;
                        btn.onClick.AddListener(() =>
                        {
                            SceneManager.LoadScene(capturedNum);
                        });
                    }
                    else
                    {
                        // 🔴 Level BỊ KHÓA: Chữ mờ đi, không bấm được
                        btn.interactable = false;
                        txt.color = new Color(1f, 1f, 1f, 0.3f);
                        frameImage.color = new Color(1f, 1f, 1f, 1f); // Vẫn giữ viền nét
                    }
                }
                // TRƯỜNG HỢP 2: Ô TRỐNG (Vượt quá totalLevels)
                else
                {

                    if (txt != null) txt.text = ""; // Xóa chữ số
                    btn.interactable = false;
                    frameImage.color = new Color(1f, 1f, 1f, 0.2f);
                }
            }

            // 3. Cập nhật thanh hiển thị Trang
            UpdatePageIndicators();
        }

        private void UpdatePageIndicators()
        {
            for (int i = 0; i < pageFrames.Length; i++)
            {
                // 1. Quản lý KHUNG RỖNG (Ẩn nếu dư trang)
                if (i >= maxPage)
                {
                    pageFrames[i].SetActive(false); // Tắt luôn cả cụm nếu game không có tới trang này
                    continue;
                }

                pageFrames[i].SetActive(true); // Bật khung rỗng lên

                // 2. Quản lý RUỘT ĐẶC (Chỉ bật ở trang hiện tại)
                if (i == currentPage - 1)
                {
                    pageFills[i].SetActive(true);  // Đang ở trang này -> Bật cục màu hồng đặc lên
                }
                else
                {
                    pageFills[i].SetActive(false); // Đang ở trang khác -> Tắt cục màu hồng đi, để lộ khung rỗng
                }
            }
        }
        public void NextPage()
        {
            if (currentPage < maxPage)
            {
                currentPage++;
                RefreshUI();
            }
        }

        public void PreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshUI();
            }
        }
    }
}
