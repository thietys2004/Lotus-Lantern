using Game.Data;
using Game.Gameplay.Player;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Core
{
    public class SoulFireManager : MonoBehaviour
    {
        public static SoulFireManager Instance { get; private set; }

        private int currentMaxLamps;
        private List<GameObject> environmentalLamps = new List<GameObject>(); // Chỉ table lamps
        private List<GameObject> allLitLamps = new List<GameObject>(); // Tất cả lamps (bao gồm lotus)

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
                Game.UI.UIManager.Instance.UpdateLanternBar(environmentalLamps.Count, currentMaxLamps);
            }
        }

        /// <summary>
        /// Thêm lamp vào danh sách. isEnvironmental = true chỉ cho table lamps.
        /// Chỉ table lamps được tính vào giới hạn max.
        /// </summary>
        public void AddLitLamp(GameObject lamp, bool isEnvironmental = true)
        {
            Debug.Log($"[SoulFireManager] AddLitLamp called - lamp: {lamp.name}, isEnvironmental: {isEnvironmental}, current env lamps: {environmentalLamps.Count}");
            allLitLamps.Add(lamp);

            if (isEnvironmental)
            {
                environmentalLamps.Add(lamp);
                Debug.Log($"[SoulFireManager] Added environmental lamp. Total env lamps: {environmentalLamps.Count}/{currentMaxLamps}");
            }

            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.UpdateLanternBar(environmentalLamps.Count, currentMaxLamps);
            }

            // Kiểm tra vượt quá giới hạn - chỉ cho environmental lamps
            if (isEnvironmental && environmentalLamps.Count > currentMaxLamps)
            {
                Debug.LogError($"[SoulFireManager] VƯỢT QUÁ MAX LAMPS ({environmentalLamps.Count} > {currentMaxLamps})! PLAYER SẼ CHẾT.");

                // Cách 1: Tìm PlayerControllerManager
                var playerManager = Object.FindFirstObjectByType<Game.Gameplay.Player.Components.PlayerControllerManager>();
                if (playerManager != null)
                {
                    Debug.LogWarning($"[SoulFireManager] ★★★ Gọi Die() qua PlayerControllerManager");
                    playerManager.Die();
                }
                // Cách 2: Fallback - tìm PlayerController
                else
                {
                    PlayerController player = Object.FindFirstObjectByType<PlayerController>();
                    if (player != null)
                    {
                        Debug.LogWarning($"[SoulFireManager] ★★★ Gọi Die() trên PlayerController. Tìm thấy: {player.gameObject.name}");
                        player.Die();
                    }
                    else
                    {
                        Debug.LogError("[SoulFireManager] ❌ Không tìm thấy PlayerController hoặc PlayerControllerManager!");
                        Debug.LogError("[SoulFireManager] ❌ Kiểm tra xem Player prefab đã được spawn chưa!");
                    }
                }
            }
        }

        public void RemoveLitLamp(GameObject lamp)
        {
            if (allLitLamps.Contains(lamp))
            {
                allLitLamps.Remove(lamp);

                // Nếu là environmental lamp, bỏ khỏi danh sách kiểm tra giới hạn
                if (environmentalLamps.Contains(lamp))
                {
                    environmentalLamps.Remove(lamp);
                }

                // Báo cho UI biết để tụt thanh màu xuống
                if (Game.UI.UIManager.Instance != null)
                {
                    Game.UI.UIManager.Instance.UpdateLanternBar(environmentalLamps.Count, currentMaxLamps);
                }
            }
        }
        public void ClearAllLamps()
        {
            ClearContainer();

            List<GameObject> lampsToClear = new List<GameObject>(allLitLamps);

            foreach (GameObject lamp in lampsToClear)
            {
                if (lamp != null)
                {
                    Game.Gameplay.Environment.LanternInteractable fixedLantern = lamp.GetComponent<Game.Gameplay.Environment.LanternInteractable>();

                    if (fixedLantern != null)
                    {
                        if (fixedLantern.isOn)
                        {
                            fixedLantern.ToggleLightOnly();
                        }
                    }
                    else
                    {
                        Destroy(lamp);
                    }
                }
            }

            environmentalLamps.Clear();
            allLitLamps.Clear();
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
                Game.UI.UIManager.Instance.UpdateLanternBar(environmentalLamps.Count, currentMaxLamps);
            }
        }

        private void Start()
        {
            // Lấy giá trị từ GameConfig
            if (GameConfig.Instance != null)
            {
                currentMaxLamps = GameConfig.Instance.MaxLampsPerLevel;
                Debug.Log($"[SoulFireManager] Initialized currentMaxLamps from GameConfig: {currentMaxLamps}");
            }
            else
            {
                currentMaxLamps = 3; // Fallback nếu GameConfig không load
                Debug.LogWarning("[SoulFireManager] GameConfig không tìm thấy, dùng default MaxLamps = 3");
            }

            // Cập nhật UI sau khi load
            Invoke(nameof(UpdateUIAfterLoad), 0.1f);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ClearAllLamps();
                Debug.Log("[SoulFireManager] Cleaned up on destroy");
            }
        }
    }
}
