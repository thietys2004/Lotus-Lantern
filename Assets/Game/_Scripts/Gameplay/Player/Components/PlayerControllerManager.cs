using Game.Core.Services;
using Game.Data;
using UnityEngine;

namespace Game.Gameplay.Player.Components
{
    /// <summary>
    /// Main player controller that orchestrates all player components.
    /// Acts as a facade for player functionality.
    /// </summary>
    public class PlayerControllerManager : MonoBehaviour
    {
        [SerializeField] private PlayerMovementComponent movementComponent;
        [SerializeField] private PlayerAnimationComponent animationComponent;
        [SerializeField] private PlayerItemComponent itemComponent;
        [SerializeField] private PlayerInteractionComponent interactionComponent;
        [SerializeField] private PlayerRespawnComponent respawnComponent;
        [SerializeField] private PlayerHazardDetectionComponent hazardDetectionComponent;

        private GameConfig config;
        private IInputService inputService;
        private IUIService uiService;

        private void Start()
        {
            // Get config
            config = GameConfig.Instance;

            // Get input service from ServiceLocator
            inputService = ServiceLocator.Instance.Get<IInputService>();

            if (inputService == null)
            {
                Debug.LogError("IInputService not registered in ServiceLocator! Attempting fallback search...");
                MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is IInputService service)
                    {
                        inputService = service;
                        ServiceLocator.Instance.Register<IInputService>(service);
                        Debug.Log($"[PlayerControllerManager] Fallback registered IInputService from {behaviour.gameObject.name}");
                        break;
                    }
                }

                if (inputService == null)
                {
                    Debug.LogError("[PlayerControllerManager] Fallback search failed: no IInputService implementation found in scene.");
                }
            }

            // Initialize respawn point
            respawnComponent.SetRespawnPoint(transform.position);

            // Subscribe to item events to update UI
            uiService = ServiceLocator.Instance.Get<IUIService>();
            if (itemComponent != null && uiService != null)
            {
                itemComponent.OnLotusCountChanged += (count) => uiService.UpdateLotusCount(count);
                itemComponent.OnLighterCountChanged += (count) => uiService.UpdateInventoryUI(
                    itemComponent.LotusCount, count, itemComponent.KeyCount);
                itemComponent.OnKeyCountChanged += (count) => uiService.UpdateInventoryUI(
                    itemComponent.LotusCount, itemComponent.LighterCount, count);
            }
        }

        private void Update()
        {
            // Check if player can move
            bool canMove = !movementComponent.IsMoving &&
                          !respawnComponent.IsRespawning &&
                          !interactionComponent.IsInteracting;

            // Handle movement input
            if (inputService != null)
            {
                Vector2 moveInput = inputService.GetMovementInput();
                if (movementComponent.TryMove(moveInput.x, moveInput.y, canMove))
                {
                    // Face the direction and update animation
                    animationComponent.FaceDirection(movementComponent.LastX);
                    animationComponent.UpdateMovementAnimation(
                        movementComponent.LastX,
                        movementComponent.LastY,
                        movementComponent.IsMoving
                    );
                }

                // Update animation with current state
                animationComponent.UpdateMovementAnimation(
                    movementComponent.LastX,
                    movementComponent.LastY,
                    movementComponent.IsMoving
                );

                // Handle place lantern input
                if (inputService.GetPlaceLanternInput() && !interactionComponent.IsInteracting)
                {
                    interactionComponent.PlaceLantern();
                }

                // Handle interact input
                if (inputService.GetInteractInput() && !interactionComponent.IsInteracting)
                {
                    interactionComponent.InteractWithEnvironment();
                }
            }
        }

        /// <summary>
        /// Kill the player and trigger respawn.
        /// </summary>
        public void Die()
        {
            if (respawnComponent.IsRespawning)
                return;

            // Play death sound
            var audioService = ServiceLocator.Instance.Get<IAudioService>();
            audioService?.PlayDeathScream();

            // Clear lanterns
            Game.Core.SoulFireManager.Instance?.ClearAllLamps();

            // Stop all actions
            movementComponent.StopMovement();
            interactionComponent.StopInteraction();

            // Show end game UI
            var uiService = ServiceLocator.Instance.Get<IUIService>();
            uiService?.ShowEndGamePanel(false);

            // Reset inventory
            itemComponent.ResetInventory();
        }

        /// <summary>
        /// Respawn the player.
        /// </summary>
        public void Respawn()
        {
            respawnComponent.Respawn();
        }

        /// <summary>
        /// Set a new respawn point (e.g., when reaching checkpoints).
        /// </summary>
        public void SetRespawnPoint(Vector3 newPoint)
        {
            respawnComponent.SetRespawnPoint(newPoint);
        }

        /// <summary>
        /// Get player inventory component.
        /// </summary>
        public PlayerItemComponent GetItemComponent()
        {
            return itemComponent;
        }

        /// <summary>
        /// Get player movement component.
        /// </summary>
        public PlayerMovementComponent GetMovementComponent()
        {
            return movementComponent;
        }

        /// <summary>
        /// Get player animation component.
        /// </summary>
        public PlayerAnimationComponent GetAnimationComponent()
        {
            return animationComponent;
        }

        /// <summary>
        /// Get player interaction component.
        /// </summary>
        public PlayerInteractionComponent GetInteractionComponent()
        {
            return interactionComponent;
        }

        /// <summary>
        /// Get player respawn component.
        /// </summary>
        public PlayerRespawnComponent GetRespawnComponent()
        {
            return respawnComponent;
        }

        /// <summary>
        /// Check if player is performing any action.
        /// </summary>
        public bool IsAnyActionInProgress()
        {
            return movementComponent.IsMoving ||
                   respawnComponent.IsRespawning ||
                   interactionComponent.IsInteracting;
        }

        /// <summary>
        /// Complete stop of all player actions.
        /// </summary>
        public void StopAllActions()
        {
            movementComponent.StopMovement();
            interactionComponent.StopInteraction();
            respawnComponent.StopRespawn();
        }
    }
}
