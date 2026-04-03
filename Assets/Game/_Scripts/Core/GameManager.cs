using UnityEngine;
namespace Core
{
    public class GameManager : MonoBehaviour
    {

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
            Debug.Log("Game Manager đã khởi động thành công!");
        }
    }
}