using Game.Gameplay.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class SoulFireManager : MonoBehaviour
    {
        public static SoulFireManager Instance { get; private set; }

        private int currentMaxLamps = 3;
        private List<GameObject> activeLamps = new List<GameObject>();
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        public void SetMaxLamps(int maxLamps)
        {
            currentMaxLamps = maxLamps;
        }
        public void AddLitLamp(GameObject lamp)
        {
            if (activeLamps.Count > currentMaxLamps)
            {
                Debug.Log("Tâm Hỏa cạn kiệt! Bạn đã đặt quá số đèn cho phép.");
                Destroy(lamp);
                PlayerController player = Object.FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.Die();
                }
                return;
            }
            activeLamps.Add(lamp);
            Debug.Log("Đã thắp đèn: " + activeLamps.Count + "/" + currentMaxLamps);

        }

        public void ClearAllLamps()
        {
            foreach (GameObject lamp in activeLamps)
            {
                if (lamp != null) Destroy(lamp);
            }
            activeLamps.Clear();
            Debug.Log("Đã dọn dẹp toàn bộ Tâm Hỏa để chơi lại từ đầu!");
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
