using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    public class KeybindingManager : MonoBehaviour
    {
        public static KeybindingManager Instance { get; private set; }

        [System.Serializable]
        public class KeyBinding
        {
            public string actionName;
            public KeyCode key;

            public KeyBinding(string name, KeyCode defaultKey)
            {
                actionName = name;
                key = defaultKey;
            }
        }

        private Dictionary<string, KeyCode> keybindings = new Dictionary<string, KeyCode>();
        private List<KeyBinding> keyBindingsList = new List<KeyBinding>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultKeybindings();
                LoadKeybindingsFromPlayerPrefs();
            }
            else Destroy(gameObject);
        }

        private void InitializeDefaultKeybindings()
        {
            // Định nghĩa các phím tắt mặc định
            AddKeybinding("Move Up", KeyCode.W);
            AddKeybinding("Move Down", KeyCode.S);
            AddKeybinding("Move Left", KeyCode.A);
            AddKeybinding("Move Right", KeyCode.D);
            AddKeybinding("Interact", KeyCode.E);
            AddKeybinding("Drop Flower", KeyCode.Space);
            AddKeybinding("Jump", KeyCode.Space);
            AddKeybinding("Pause", KeyCode.Escape);
            AddKeybinding("Inventory", KeyCode.I);
        }

        public void AddKeybinding(string actionName, KeyCode defaultKey)
        {
            if (!keybindings.ContainsKey(actionName))
            {
                keybindings[actionName] = defaultKey;
                keyBindingsList.Add(new KeyBinding(actionName, defaultKey));
            }
        }

        public KeyCode GetKeybinding(string actionName)
        {
            return keybindings.ContainsKey(actionName) ? keybindings[actionName] : KeyCode.None;
        }

        // --- HÀM MỚI: Kiểm tra xem phím đã được gán cho hành động nào chưa ---
        public bool IsKeyAlreadyBound(KeyCode checkKey, out string conflictingAction)
        {
            conflictingAction = string.Empty;

            // Bỏ qua nếu phím là None
            if (checkKey == KeyCode.None) return false;

            foreach (var kvp in keybindings)
            {
                if (kvp.Value == checkKey)
                {
                    conflictingAction = kvp.Key;
                    return true;
                }
            }
            return false;
        }

        // --- CẬP NHẬT: Trả về kiểu bool để UI biết là gán thành công hay thất bại ---
        public bool SetKeybinding(string actionName, KeyCode newKey)
        {
            if (keybindings.ContainsKey(actionName))
            {
                // 1. Kiểm tra trùng lặp phím
                if (IsKeyAlreadyBound(newKey, out string conflictingAction))
                {
                    // Nếu phím trùng với chính hành động hiện tại thì bỏ qua, coi như thành công
                    if (conflictingAction == actionName) return true;

                    // Nếu trùng với hành động khác, báo lỗi và từ chối gán
                    Debug.LogWarning($"Phím '{newKey}' đã được sử dụng cho hành động '{conflictingAction}'. Không thể gán trùng!");
                    return false;
                }

                // 2. Gán phím mới vào Dictionary
                keybindings[actionName] = newKey;

                // 3. (Sửa lỗi code cũ): Cập nhật phím mới vào List để hiển thị UI không bị sai
                KeyBinding bindingInList = keyBindingsList.Find(b => b.actionName == actionName);
                if (bindingInList != null)
                {
                    bindingInList.key = newKey;
                }

                // 4. Lưu lại
                SaveKeybindingsToPlayerPrefs();
                return true; // Gán thành công
            }
            return false; // Hành động không tồn tại
        }

        public List<KeyBinding> GetAllKeybindings()
        {
            return keyBindingsList;
        }

        private void SaveKeybindingsToPlayerPrefs()
        {
            foreach (var binding in keybindings)
            {
                PlayerPrefs.SetString($"KB_{binding.Key}", binding.Value.ToString());
            }
            PlayerPrefs.Save();
        }

        private void LoadKeybindingsFromPlayerPrefs()
        {
            // 1. Tạo một danh sách phụ (bản sao) chứa tên các hành động để tránh lỗi Collection Modified
            List<string> actionNames = new List<string>(keybindings.Keys);

            // 2. Chạy vòng lặp qua danh sách phụ
            foreach (string actionName in actionNames)
            {
                string prefKey = $"KB_{actionName}";
                if (PlayerPrefs.HasKey(prefKey))
                {
                    string keyString = PlayerPrefs.GetString(prefKey);
                    if (System.Enum.TryParse<KeyCode>(keyString, out KeyCode loadedKey))
                    {
                        // Gán đè vào bản chính an toàn
                        keybindings[actionName] = loadedKey;
                    }
                }
            }

            // 3. Cập nhật lại list hiển thị
            for (int i = 0; i < keyBindingsList.Count; i++)
            {
                keyBindingsList[i].key = keybindings[keyBindingsList[i].actionName];
            }
        }

        public void ResetToDefaults()
        {
            keybindings.Clear();
            keyBindingsList.Clear();
            InitializeDefaultKeybindings();
            SaveKeybindingsToPlayerPrefs();
        }
    }
}