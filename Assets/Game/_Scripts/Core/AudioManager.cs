using UnityEngine;
namespace Game.Core
{
    public class AudioManager : MonoBehaviour
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

                if (!walkSource.isPlaying) walkSource.Play();
            }
            else
            {

                if (walkSource.isPlaying) walkSource.Pause();
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
    }
}
