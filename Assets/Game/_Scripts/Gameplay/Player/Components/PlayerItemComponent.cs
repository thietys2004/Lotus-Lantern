using UnityEngine;

namespace Game.Gameplay.Player.Components
{
    /// <summary>
    /// Manages player inventory (items collected).
    /// </summary>
    public class PlayerItemComponent : MonoBehaviour
    {
        private int lotusCount = 0;
        private int lighterCount = 0;
        private int keyCount = 0;

        // Callbacks for UI updates
        public event System.Action<int> OnLotusCountChanged;
        public event System.Action<int> OnLighterCountChanged;
        public event System.Action<int> OnKeyCountChanged;

        public int LotusCount => lotusCount;
        public int LighterCount => lighterCount;
        public int KeyCount => keyCount;

        /// <summary>
        /// Add a lotus to inventory.
        /// </summary>
        public void AddLotus(int amount = 1)
        {
            lotusCount += amount;
            OnLotusCountChanged?.Invoke(lotusCount);
        }

        /// <summary>
        /// Remove a lotus from inventory.
        /// </summary>
        public bool ConsumeLotus(int amount = 1)
        {
            if (lotusCount >= amount)
            {
                lotusCount -= amount;
                OnLotusCountChanged?.Invoke(lotusCount);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a lighter to inventory.
        /// </summary>
        public void AddLighter(int amount = 1)
        {
            lighterCount += amount;
            OnLighterCountChanged?.Invoke(lighterCount);
        }

        /// <summary>
        /// Check if player has a lighter.
        /// </summary>
        public bool HasLighter()
        {
            return lighterCount > 0;
        }

        /// <summary>
        /// Remove a lighter from inventory.
        /// </summary>
        public bool ConsumeLighter(int amount = 1)
        {
            if (lighterCount >= amount)
            {
                lighterCount -= amount;
                OnLighterCountChanged?.Invoke(lighterCount);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a key to inventory.
        /// </summary>
        public void AddKey(int amount = 1)
        {
            keyCount += amount;
            OnKeyCountChanged?.Invoke(keyCount);
        }

        /// <summary>
        /// Remove a key from inventory.
        /// </summary>
        public bool ConsumeKey(int amount = 1)
        {
            if (keyCount >= amount)
            {
                keyCount -= amount;
                OnKeyCountChanged?.Invoke(keyCount);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reset all inventory.
        /// </summary>
        public void ResetInventory()
        {
            lotusCount = 0;
            lighterCount = 0;
            keyCount = 0;

            OnLotusCountChanged?.Invoke(lotusCount);
            OnLighterCountChanged?.Invoke(lighterCount);
            OnKeyCountChanged?.Invoke(keyCount);
        }
    }
}
