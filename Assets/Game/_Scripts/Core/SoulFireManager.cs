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

        public Transform safePathContainer; // Tham chiếu đến Container để dọn dẹp sau này
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        public void SetMaxLamps(int maxLamps)
        {
            currentMaxLamps = maxLamps;
            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.UpdateLanternBar(activeLamps.Count, currentMaxLamps);
            }
        }
        public void AddLitLamp(GameObject lamp)

        {
            activeLamps.Add(lamp);

            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.UpdateLanternBar(activeLamps.Count, currentMaxLamps);
            }


            if (activeLamps.Count > currentMaxLamps)
            {

                PlayerController player = Object.FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.Die();
                }
            }


        }
        public void RemoveLitLamp(GameObject lamp)
        {
            if (activeLamps.Contains(lamp))
            {
                activeLamps.Remove(lamp); // Bớt đi 1 đèn

                // Báo cho UI biết để tụt thanh màu xuống
                if (Game.UI.UIManager.Instance != null)
                {
                    Game.UI.UIManager.Instance.UpdateLanternBar(activeLamps.Count, currentMaxLamps);
                }


            }
        }
        public void ClearAllLamps()
        {

            ClearContainer();


            List<GameObject> lampsToClear = new List<GameObject>(activeLamps);

            foreach (GameObject lamp in lampsToClear)
            {
                if (lamp != null)
                {
                    Game.Gameplay.Environment.LanternInteractable fixedLantern = lamp.GetComponent<Game.Gameplay.Environment.LanternInteractable>();

                    if (fixedLantern != null)
                    {
                        if (fixedLantern.isOn)
                        {
                            fixedLantern.ToggleLantern(); // Chỉ tắt đi, không xóa
                        }
                    }
                    else
                    {
                        Destroy(lamp); // Đèn tạm thời thì xóa
                    }
                }
            }

            activeLamps.Clear();


            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.UpdateLanternBar(activeLamps.Count, currentMaxLamps);
                Game.UI.UIManager.Instance.ResetSteps();
            }
        }
        public void ClearContainer()
        {
            if (safePathContainer != null)
            {

                foreach (Transform child in safePathContainer)
                {

                    Destroy(child.gameObject);
                }

            }
        }
        private void UpdateUIAfterLoad()
        {
            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.UpdateLanternBar(activeLamps.Count, currentMaxLamps);
            }

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Invoke(nameof(UpdateUIAfterLoad), 0.1f);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
