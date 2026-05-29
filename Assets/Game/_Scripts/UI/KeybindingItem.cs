using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class KeybindingItem : MonoBehaviour
    {
        public TextMeshProUGUI actionNameText;
        public Button rebindButton;
        public TextMeshProUGUI rebindButtonText;

        private string actionName;
        private KeyCode currentKey;
        private bool isWaitingForInput = false;

        private void Start()
        {
            if (rebindButton != null)
            {
                rebindButton.onClick.AddListener(OnRebindClicked);
            }
        }

        public void Initialize(string actionName, KeyCode currentKey)
        {
            this.actionName = actionName;
            this.currentKey = currentKey;

            if (actionNameText != null)
                actionNameText.text = actionName;

            UpdateButtonText();
        }

        private void OnRebindClicked()
        {
            if (!isWaitingForInput)
            {
                isWaitingForInput = true;
                if (rebindButtonText != null)
                    rebindButtonText.text = "Waiting...";
                rebindButton.interactable = false;
            }
        }

        private void Update()
        {
            if (isWaitingForInput)
            {
                // Kiểm tra tất cả các phím được nhấn
                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode) && keyCode != KeyCode.Escape)
                    {
                        AssignNewKey(keyCode);
                        return;
                    }
                }

                // Nhấn Escape để hủy
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelRebind();
                }
            }
        }

        private void AssignNewKey(KeyCode newKey)
        {
            currentKey = newKey;
            Game.Core.KeybindingManager.Instance.SetKeybinding(actionName, newKey);
            isWaitingForInput = false;
            rebindButton.interactable = true;
            UpdateButtonText();
        }

        private void CancelRebind()
        {
            isWaitingForInput = false;
            rebindButton.interactable = true;
            UpdateButtonText();
        }

        private void UpdateButtonText()
        {
            if (rebindButtonText != null)
                rebindButtonText.text = currentKey.ToString();
        }
    }
}
