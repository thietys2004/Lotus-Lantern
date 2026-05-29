using UnityEngine;

namespace Game.UI
{
    public class OptionsPanel : MonoBehaviour
    {
        public GameObject optionsPanel;
        public KeybindingUI keybindingUI;

        [Header("Options Buttons")]
        public UnityEngine.UI.Button keybindingsButton;
        public UnityEngine.UI.Button videoSettingsButton;
        public UnityEngine.UI.Button audioSettingsButton;
        public UnityEngine.UI.Button closeButton;

        private void Start()
        {
            if (keybindingsButton != null)
                keybindingsButton.onClick.AddListener(() => keybindingUI.OpenKeybindingPanel());

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseOptions);
        }

        public void OpenOptions()
        {
            if (optionsPanel != null)
                optionsPanel.SetActive(true);
        }

        public void CloseOptions()
        {
            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }
    }
}
