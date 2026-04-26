using Game.Core;
using UnityEngine;
namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public LevelLoader levelLoader;
        private int currentLevelIndex = 1;
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        void Start()
        {
            StartLevel(currentLevelIndex);
            Debug.Log("Game Manager đã khởi động thành công!");
        }
        public void StartLevel(int index)
    {
        string mapName = "Map_Tu_Dong_" + index.ToString("D2");
        levelLoader.LoadLevel(mapName);
    }
    public void NextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex <= 10)
        {
            StartLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log("Chúc mừng! Bạn đã thoát khỏi mê cung.");
        }
    }
    }
}