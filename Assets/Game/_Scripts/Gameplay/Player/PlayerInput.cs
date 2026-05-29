using Game.Core.Services;
using UnityEngine;

namespace Game.Gameplay.Player
{
    /// <summary>
    /// Handles player input abstraction.
    /// Can be easily swapped for mobile input, gamepad, etc.
    /// </summary>
    public class PlayerInput : MonoBehaviour, IInputService
    {
        private void Start()
        {
            // Đảm bảo KeybindingManager được khởi tạo
            if (Game.Core.KeybindingManager.Instance == null)
            {
                Debug.LogWarning("KeybindingManager không được tìm thấy!");
            }
        }

        public Vector2 GetMovementInput()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            // Prevent diagonal movement
            if (x != 0)
                y = 0;

            return new Vector2(x, y);
        }

        public bool GetPlaceLanternInput()
        {
            KeyCode key = Game.Core.KeybindingManager.Instance.GetKeybinding("Drop Flower");
            return Input.GetKeyDown(key);
        }

        public bool GetInteractInput()
        {
            KeyCode key = Game.Core.KeybindingManager.Instance.GetKeybinding("Interact");
            return Input.GetKeyDown(key);
        }

        public bool GetPauseInput()
        {
            KeyCode key = Game.Core.KeybindingManager.Instance.GetKeybinding("Pause");
            return Input.GetKeyDown(key);
        }

        private void OnEnable()
        {
            ServiceLocator.Instance.Register<IInputService>(this);
        }

        private void OnDisable()
        {
            if (ServiceLocator.Instance != null)
                ServiceLocator.Instance.Unregister<IInputService>();
        }
    }
}
