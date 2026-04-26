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
        [SerializeField] private KeyCode placeLanternKey = KeyCode.Space;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        public Vector2 GetMovementInput()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            // Prevent diagonal movement
            if (x != 0)
                y = 0;

            return new Vector2(x, y);
        }

        public bool GetPlaceLanternInput() => Input.GetKeyDown(placeLanternKey);
        public bool GetInteractInput() => Input.GetKeyDown(interactKey);
        public bool GetPauseInput() => Input.GetKeyDown(pauseKey);

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
