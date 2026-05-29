using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ConfirmDialog : MonoBehaviour
    {
        public static ConfirmDialog Instance { get; private set; }

        public GameObject dialogPanel;
        public TextMeshProUGUI messageText;
        public Button confirmButton;
        public Button cancelButton;

        private System.Action onConfirmCallback;
        private System.Action onCancelCallback;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }

        public void ShowConfirm(string message, System.Action onConfirm, System.Action onCancel = null)
        {
            if (messageText != null)
                messageText.text = message;

            onConfirmCallback = onConfirm;
            onCancelCallback = onCancel;

            if (dialogPanel != null)
                dialogPanel.SetActive(true);
        }

        private void OnConfirmClicked()
        {
            onConfirmCallback?.Invoke();
            CloseDialog();
        }

        private void OnCancelClicked()
        {
            onCancelCallback?.Invoke();
            CloseDialog();
        }

        private void CloseDialog()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }
    }
}
