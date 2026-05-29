using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class KeybindingUI : MonoBehaviour
    {
        [Header("Keybinding Panel")]
        public GameObject keybindingPanel;
        public Transform keybindingContent; // ScrollView Content
        public ScrollRect scrollRect;

        [Header("Prefab")]
        public GameObject keybindingItemPrefab;

        [Header("Buttons")]
        public Button closeButton;
        public Button resetDefaultsButton;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseKeybindingPanel);

            if (resetDefaultsButton != null)
                resetDefaultsButton.onClick.AddListener(ResetToDefaults);

            // Đảm bảo panel bắt đầu ở trạng thái ẩn
            if (keybindingPanel != null)
                keybindingPanel.SetActive(false);
        }

        public void OpenKeybindingPanel()
        {
            if (keybindingPanel != null)
            {
                keybindingPanel.SetActive(true);
                PopulateKeybindingList();

                // Scroll lên đầu
                if (scrollRect != null)
                    scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void CloseKeybindingPanel()
        {
            if (keybindingPanel != null)
                keybindingPanel.SetActive(false);
        }

        private void PopulateKeybindingList()
        {
            // Xóa các item cũ
            foreach (Transform child in keybindingContent)
            {
                Destroy(child.gameObject);
            }

            // Lấy tất cả keybindings từ manager
            var allKeybindings = Game.Core.KeybindingManager.Instance.GetAllKeybindings();

            // Tạo item cho mỗi keybinding
            foreach (var binding in allKeybindings)
            {
                GameObject itemGO = Instantiate(keybindingItemPrefab, keybindingContent);
                KeybindingItem item = itemGO.GetComponent<KeybindingItem>();

                if (item != null)
                {
                    item.Initialize(binding.actionName, binding.key);
                }
            }
        }

        private void ResetToDefaults()
        {
            ConfirmDialog.Instance.ShowConfirm(
                "Reset to Default Keybindings?",
                onConfirm: () =>
                {
                    Game.Core.KeybindingManager.Instance.ResetToDefaults();
                    PopulateKeybindingList();
                },
                onCancel: null
            );
        }
    }
}
