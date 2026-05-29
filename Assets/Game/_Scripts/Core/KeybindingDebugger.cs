using UnityEngine;
using Game.Core;

public class KeybindingDebugger : MonoBehaviour
{
    private void Update()
    {
        // Bấm P để kiểm tra phím hiện tại
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("====== KEYBINDING DEBUG ======");
            Debug.Log($"Drop Flower Key: {KeybindingManager.Instance.GetKeybinding("Drop Flower")}");
            Debug.Log($"Interact Key: {KeybindingManager.Instance.GetKeybinding("Interact")}");
            Debug.Log($"Pause Key: {KeybindingManager.Instance.GetKeybinding("Pause")}");
            Debug.Log("==============================");
        }

        // Bấm R để reset PlayerPrefs và test reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("✅ Đã xóa tất cả PlayerPrefs!");
        }
    }
}
