using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using System.Collections.Generic;

// THÊM THƯ VIỆN NÀY ĐỂ DÙNG EDITOR
#if UNITY_EDITOR
using UnityEditor;
#endif

public class KeybindUIManager : MonoBehaviour
{
    [System.Serializable]
    public struct KeyIcon
    {
        public KeyCode key;
        public Sprite icon;
    }

    [Header("List icon tự động điền")]
    public List<KeyIcon> keyIconsList = new List<KeyIcon>();

    [Header("UI Elements")]
    public Image interactKeyImage;
    public Image dropFlowerKeyImage;
    public TextMeshProUGUI warningText;
    public Sprite defaultUnknownIcon;

    private string currentEditingAction = null;

    void Start()
    {
        if (warningText != null) warningText.text = "";
        UpdateUI();
    }

    // =========================================================
    // NÚT BẤM MA THUẬT: TỰ ĐỘNG LẤY ẢNH TỪ THƯ MỤC
    // =========================================================
    // =========================================================
    // NÚT BẤM MA THUẬT: TỰ ĐỘNG LẤY ẢNH TỪ THƯ MỤC
    // =========================================================
#if UNITY_EDITOR
    [ContextMenu("⚡ Tự Động Load Icon Từ Thư Mục")]
    public void AutoLoadIconsFromFolder()
    {
        keyIconsList.Clear();

        // Đường dẫn thư mục của bạn
        string folderPath = "Assets/Game/Art/UI/Icon/Key";

        // Quét từ A đến Z (26 ký tự)
        for (int i = 0; i < 26; i++)
        {
            KeyCode code = (KeyCode)(97 + i);

            // Bỏ qua hoàn toàn chữ V (i = 21) vì không có ảnh
            if (code == KeyCode.V)
            {
                continue; 
            }

            // Xử lý dồn số: 
            // Nếu chữ cái trước chữ V (A -> U) thì tên file là i + 1
            // Nếu chữ cái sau chữ V (W -> Z) thì tên file là i (do bị tụt mất 1 số)
            int fileIndex = (i < 21) ? (i + 1) : i;

            // Tạo đường dẫn
            string path = $"{folderPath}/L. Key {fileIndex}.png";
            
            // Lấy ảnh
            Sprite loadedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (loadedSprite != null)
            {
                keyIconsList.Add(new KeyIcon { key = code, icon = loadedSprite });
            }
            else
            {
                Debug.LogWarning($"[Thiếu Ảnh] Không tìm thấy ảnh tại: {path} cho phím {code}");
            }
        }

        Debug.Log($"<color=green>Đã load thành công {keyIconsList.Count} icon vào List!</color>");
        
        // Lưu lại thay đổi
        EditorUtility.SetDirty(this); 
    }
#endif
    // =========================================================
    // =========================================================

    // Hàm lấy ảnh ra để hiển thị (giữ nguyên như trước)
    private Sprite GetIconForKey(KeyCode code)
    {
        foreach (var item in keyIconsList)
        {
            if (item.key == code) return item.icon;
        }
        return defaultUnknownIcon;
    }

    // Các hàm UpdateUI, StartRebind, OnGUI... của bạn giữ nguyên ở dưới
    void UpdateUI()
    {
        KeyCode interactCode = KeybindingManager.Instance.GetKeybinding("Interact");
        KeyCode dropFlowerCode = KeybindingManager.Instance.GetKeybinding("Drop Flower");

        interactKeyImage.sprite = GetIconForKey(interactCode);
        dropFlowerKeyImage.sprite = GetIconForKey(dropFlowerCode);
    }

    public void StartRebindInteract()
    {
        currentEditingAction = "Interact";
        interactKeyImage.sprite = defaultUnknownIcon;

        // 1. Ép Unity bật (On) cái Warning Text lên đề phòng bạn đang tắt nó
        if (warningText != null) warningText.gameObject.SetActive(true);

        // 2. Hiện câu nhắc nhở
        warningText.text = "Đang chờ... Vui lòng bấm một phím bất kỳ!";
        warningText.color = Color.yellow; // Đổi màu vàng cho thân thiện
    }

    public void StartRebindDropFlower()
    {
        currentEditingAction = "Drop Flower";
        dropFlowerKeyImage.sprite = defaultUnknownIcon;

        if (warningText != null) warningText.gameObject.SetActive(true);
        warningText.text = "Đang chờ... Vui lòng bấm một phím bất kỳ!";
        warningText.color = Color.yellow;
    }

    // --- LẮNG NGHE BÀN PHÍM ---
    void OnGUI()
    {
        if (currentEditingAction != null)
        {
            Event e = Event.current;
            if (e.isKey && e.keyCode != KeyCode.None)
            {
                // ==========================================
                // PHA GIAN LẬN: BẮT QUẢ TANG BẤM CHỮ V =))
                // ==========================================
                if (e.keyCode == KeyCode.V)
                {
                    warningText.text = "đã có :))"; // Hiện câu trêu tức
                    currentEditingAction = null;     // Hủy trạng thái chờ nhập phím
                    UpdateUI();                      // Load lại ảnh cũ
                    return;                          // Thoát luôn, không cho chạy code gán phím bên dưới nữa!
                }
                // ==========================================

                // Nếu không phải chữ V thì cho gán phím bình thường
                bool success = KeybindingManager.Instance.SetKeybinding(currentEditingAction, e.keyCode);

                if (success)
                {
                    // Nếu gán thành công -> Tắt luôn cái dòng thông báo đi cho gọn
                    warningText.gameObject.SetActive(false);
                }
                else
                {
                    // Nếu gán trùng -> Vẫn bật, nhưng hiện chữ Đỏ cảnh báo
                    warningText.gameObject.SetActive(true);
                    warningText.color = Color.red;
                    KeybindingManager.Instance.IsKeyAlreadyBound(e.keyCode, out string conflictAction);
                    warningText.text = $"Không thể gán! Phím đang dùng cho '{conflictAction}'";
                }

                currentEditingAction = null;
                UpdateUI();
            }
        }
    }
}