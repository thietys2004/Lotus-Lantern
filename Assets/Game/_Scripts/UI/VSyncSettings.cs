using UnityEngine;

public class VSyncSettings : MonoBehaviour
{
    // Hàm này được gọi khi người chơi tick/untick vào ô V-Sync trong Options
    public void SetVSync(bool isVSyncOn)
    {
        if (isVSyncOn)
        {
            // Bật V-Sync (khóa FPS theo tần số màn hình, ví dụ 60Hz -> 60 FPS)
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            // Tắt V-Sync (FPS chạy thả ga)
            QualitySettings.vSyncCount = 0;

            // Tùy chọn: Khi tắt V-Sync, bạn có thể giới hạn FPS tối đa để máy không bị quá nóng
            // Application.targetFrameRate = 120; 
        }
    }
}