using UnityEngine;
using System.Collections.Generic;
using TMPro; // Dùng cho Dropdown của TextMeshPro

public class ResolutionSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    void Start()
    {
        // Lấy danh sách các độ phân giải mà màn hình của người chơi hỗ trợ
        resolutions = Screen.resolutions;

        // Xóa các lựa chọn cũ trong Dropdown
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // Đưa các độ phân giải vào danh sách chọn
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Kiểm tra xem đâu là độ phân giải hiện tại đang dùng
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        // Đổ dữ liệu vào UI Dropdown
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // Hàm này được gọi khi người chơi chọn một độ phân giải khác trong Options
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        // Áp dụng độ phân giải mới (tham số thứ 3 là chế độ Fullscreen)
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}