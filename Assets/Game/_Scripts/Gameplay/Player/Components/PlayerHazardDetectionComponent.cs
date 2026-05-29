using Game.Core.Services;
using UnityEngine;

namespace Game.Gameplay.Player.Components
{
    /// <summary>
    /// Detects collision with hazards (Miasma zones, obstacles, etc.)
    /// and handles player damage/respawn logic.
    /// </summary>
    public class PlayerHazardDetectionComponent : MonoBehaviour
    {
        private PlayerControllerManager playerControllerManager;
        private PlayerMovementComponent movementComponent;
        private IAudioService audioService;
        private IUIService uiService;

        public event System.Action OnHazardDetected;
        public event System.Action OnPlayerDamaged;

        private void Start()
        {
            playerControllerManager = GetComponent<PlayerControllerManager>();
            movementComponent = GetComponent<PlayerMovementComponent>();
            audioService = ServiceLocator.Instance.Get<IAudioService>();
            uiService = ServiceLocator.Instance.Get<IUIService>();

            if (playerControllerManager == null)
            {
                Debug.LogError("[PlayerHazardDetection] PlayerControllerManager not found!");
            }

            if (movementComponent == null)
            {
                Debug.LogError("[PlayerHazardDetection] PlayerMovementComponent not found!");
            }
        }

        /// <summary>
        /// Detect collision with hazards.
        /// </summary>
        private void OnTriggerStay2D(Collider2D collision)
        {
            // Check if colliding with hazard
            if (collision.CompareTag("Hazard"))
            {
                HandleHazardCollision(collision);
            }
        }

        /// <summary>
        /// Handle hazard collision - check if on safe path before triggering death.
        /// </summary>
        private void HandleHazardCollision(Collider2D hazardCollider)
        {
            // Check if player is on a safe path - if so, ignore hazard
            if (movementComponent != null && movementComponent.IsSafePath())
            {
                return;
            }

            // Invoke event for listeners
            OnHazardDetected?.Invoke();
            OnPlayerDamaged?.Invoke();

            // Trigger death with full logic (sound, UI, inventory reset, etc.)
            if (playerControllerManager != null)
            {
                playerControllerManager.Die();
            }
            else
            {
                Debug.LogError("[PlayerHazard] PlayerControllerManager not found - cannot trigger death!");
            }
        }

        /// <summary>
        /// Check if player is currently in a hazard zone (using OnTriggerStay2D equivalent).
        /// </summary>
        public bool IsInHazardZone()
        {
            // Cast a circle at player position to check for hazards
            Collider2D[] hazards = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach (Collider2D hazard in hazards)
            {
                if (hazard.CompareTag("Hazard"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
