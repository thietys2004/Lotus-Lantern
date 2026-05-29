using UnityEngine;
using Game.Core.Services;

namespace Game.Core
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        public static AudioManager Instance { get; private set; }
        public AudioSource bgmSource;
        public AudioSource sfxSource;
        public AudioSource walkSource;

        public AudioClip walkClip;
        public AudioClip bgmMusic;
        public AudioClip fireClip;
        public AudioClip pickupClip;
        public AudioClip doorClip;
        public AudioClip lotusClip;
        public AudioClip deathScreamClip;

        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
                sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                ApplyAudioSettings();

                // Register with ServiceLocator for dependency injection
                ServiceLocator.Instance.Register<IAudioService>(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            if (bgmMusic != null && bgmSource != null)
            {
                bgmSource.clip = bgmMusic;
                bgmSource.loop = true;
                bgmSource.Play();


            }
            if (walkClip != null && walkSource != null)
            {
                walkSource.clip = walkClip;
                walkSource.loop = true;
            }
        }

        public void PlaySound(string clipName)
        {
            // Generic method to play sound by clip name
            // This can be extended to use a dictionary of clip names if needed
            // For now, map common names to specific methods
            switch (clipName.ToLower())
            {
                case "walking":
                    SetWalkingSound(true);
                    break;
                case "stop":
                    SetWalkingSound(false);
                    break;
                case "fire":
                    PlayFireSound();
                    break;
                case "pickup":
                    PlayPickupSound();
                    break;
                case "door":
                    PlayDoorSound();
                    break;
                case "lotus":
                    PlayLotusSound();
                    break;
                case "death":
                    PlayDeathScream();
                    break;
                default:
                    Debug.LogWarning($"[AudioManager] Unknown sound clip: {clipName}");
                    break;
            }
        }

        public void PlayFireSound()
        {
            if (fireClip != null) sfxSource.PlayOneShot(fireClip);
        }

        public void PlayPickupSound()
        {
            if (pickupClip != null) sfxSource.PlayOneShot(pickupClip);
        }

        public void PlayDoorSound()
        {
            if (doorClip != null) sfxSource.PlayOneShot(doorClip);
        }
        //public void PlayWalkSound()
        //{
        //    if (walkClip != null) sfxSource.PlayOneShot(walkClip);
        //}
        public void SetWalkingSound(bool isWalking)
        {
            if (walkSource == null || walkClip == null) return;

            if (isWalking)
            {
                if (!walkSource.isPlaying)
                {
                    walkSource.Play();
                    Debug.Log("AudioManager: WalkSource Started"); // Kiểm tra xem code có chạy vào đây không
                }
            }
            else
            {
                if (walkSource.isPlaying)
                {
                    walkSource.Stop(); // Dùng Stop để lần sau kêu lại từ đầu clip
                    Debug.Log("AudioManager: WalkSource Stopped");
                }
            }
        }
        public void PlayLotusSound()
        {
            if (lotusClip != null) sfxSource.PlayOneShot(lotusClip);
        }
        public void PlayDeathScream()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }


            if (sfxSource != null)
            {
                sfxSource.Stop();
            }


            if (deathScreamClip != null)
            {
                sfxSource.PlayOneShot(deathScreamClip);
            }
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = volume;
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.Save();
            ApplyAudioSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = volume;
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
            ApplyAudioSettings();
        }

        private void ApplyAudioSettings()
        {

            if (bgmSource != null) bgmSource.volume = bgmVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (walkSource != null) walkSource.volume = sfxVolume;
        }

        public float GetBGMVolume()
        {
            return bgmVolume;
        }

        public float GetSFXVolume()
        {
            return sfxVolume;
        }
    }
}
