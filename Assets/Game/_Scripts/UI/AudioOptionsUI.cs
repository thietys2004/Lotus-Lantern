using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Handles audio settings UI (Sliders for Music and Sound volume)
    /// Connects UI Sliders to AudioManager and saves settings via PlayerPrefs
    /// </summary>
    public class AudioOptionsUI : MonoBehaviour
    {
        [Header("Audio Sliders - Kéo 2 cái Slider vào đây")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundSlider;

        private Game.Core.AudioManager audioManager;

        private void Start()
        {
            // Tìm AudioManager trong scene
            audioManager = FindObjectOfType<Game.Core.AudioManager>();

            if (audioManager == null)
            {
                Debug.LogError("[AudioOptionsUI] AudioManager not found in scene!");
                return;
            }

            // Thiết lập giá trị Slider khớp với âm lượng đã lưu
            SetupMusicSlider();
            SetupSoundSlider();
        }

        private void SetupMusicSlider()
        {
            if (musicSlider == null)
            {
                Debug.LogWarning("[AudioOptionsUI] Music Slider not assigned!");
                return;
            }

            // Đặt Min/Max Value (âm lượng từ 0 đến 1)
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;

            // Lấy giá trị âm lượng đã lưu từ PlayerPrefs
            float savedVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            musicSlider.value = savedVolume;

            // Gắn sự kiện: Bất cứ khi nào kéo cục gạt, gọi hàm SetMusic
            musicSlider.onValueChanged.AddListener(SetMusic);
        }

        private void SetupSoundSlider()
        {
            if (soundSlider == null)
            {
                Debug.LogWarning("[AudioOptionsUI] Sound Slider not assigned!");
                return;
            }

            // Đặt Min/Max Value
            soundSlider.minValue = 0f;
            soundSlider.maxValue = 1f;

            // Lấy giá trị âm lượng đã lưu
            float savedVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            soundSlider.value = savedVolume;

            // Gắn sự kiện
            soundSlider.onValueChanged.AddListener(SetSound);
        }

        /// <summary>
        /// Được gọi tự động khi kéo Music Slider
        /// </summary>
        private void SetMusic(float value)
        {
            if (audioManager != null)
            {
                audioManager.SetBGMVolume(value);
                Debug.Log($"[AudioOptionsUI] Music Volume set to: {value:F2}");
            }
        }

        /// <summary>
        /// Được gọi tự động khi kéo Sound Slider
        /// </summary>
        private void SetSound(float value)
        {
            if (audioManager != null)
            {
                audioManager.SetSFXVolume(value);
                Debug.Log($"[AudioOptionsUI] Sound Volume set to: {value:F2}");
            }
        }
    }
}
