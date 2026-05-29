using Game.Data;
using System.Collections;
using Game.Core.Services;
using UnityEngine;

namespace Game.Gameplay.Player.Components
{
    /// <summary>
    /// Handles player interactions with environment and items.
    /// </summary>
    public class PlayerInteractionComponent : MonoBehaviour
    {
        [SerializeField] private GameObject lotusPrefab;

        private GameConfig config;
        private PlayerAnimationComponent animationComponent;
        private PlayerItemComponent itemComponent;
        private PlayerMovementComponent movementComponent;

        private GameObject currentLotus;
        private bool isInteracting = false;

        public bool IsInteracting => isInteracting;

        private void Start()
        {
            config = GameConfig.Instance;
            animationComponent = GetComponent<PlayerAnimationComponent>();
            itemComponent = GetComponent<PlayerItemComponent>();
            movementComponent = GetComponent<PlayerMovementComponent>();
        }

        /// <summary>
        /// Place a lotus lantern (requires lotus in inventory).
        /// </summary>
        public void PlaceLantern()
        {
            if (itemComponent.ConsumeLotus())
            {
                StartCoroutine(PlaceLanternRoutine());
            }
        }

        /// <summary>
        /// Routine for placing lantern with animation and sound.
        /// </summary>
        private IEnumerator PlaceLanternRoutine()
        {
            isInteracting = true;
            animationComponent.SetInteracting(true);

            // Play sound
            var audioService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IAudioService>();
            audioService?.PlaySound("Lotus");

            yield return new WaitForSeconds(config.LanternSpawnDelay);

            // Destroy previous lotus
            if (currentLotus != null)
                Destroy(currentLotus);

            // Calculate snapped position
            Vector3 rawFeetPos = transform.position + config.LotusSpawnOffset;
            float snapX = Mathf.Floor(rawFeetPos.x) + config.LanternGridSnapOffset;
            float snapY = Mathf.Floor(rawFeetPos.y) + config.LanternGridSnapOffset;
            Vector3 snappedPos = new Vector3(snapX, snapY, 0f);

            // Spawn lotus
            currentLotus = Instantiate(lotusPrefab, snappedPos, Quaternion.identity);

            // Activate lantern
            var lantern = currentLotus.GetComponent<Game.Gameplay.Skill.LotusLantern>();
            if (lantern != null)
            {
                lantern.ActivateLantern(movementComponent.GetCurrentDirection());
            }

            yield return new WaitForSeconds(config.LanternActivateDelay);

            // Update UI
            var uiService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IUIService>();
            uiService?.AddStep();

            animationComponent.SetInteracting(false);
            isInteracting = false;
        }

        /// <summary>
        /// Interact with environment (pick items, toggle lanterns).
        /// </summary>
        public void InteractWithEnvironment()
        {
            if (isInteracting || movementComponent.IsMoving)
                return;

            Vector3 facingDirection = movementComponent.GetCurrentDirection();
            Vector3 targetInteractPos = transform.position + (facingDirection * config.GridSize);

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
                targetInteractPos,
                config.PickupActivationRadius
            );

            // Check for items first
            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag("LotusItem"))
                {
                    StartCoroutine(PickupRoutine(col.gameObject, "Lotus"));
                    return;
                }

                if (col.CompareTag("LighterItem"))
                {
                    StartCoroutine(PickupRoutine(col.gameObject, "Lighter"));
                    return;
                }

                if (col.CompareTag("KeyItem"))
                {
                    StartCoroutine(PickupRoutine(col.gameObject, "Key"));
                    return;
                }

                if (col.CompareTag("Finish"))
                {
                    if (itemComponent.KeyCount > 0)
                    {
                        Debug.Log("[Interaction] Opened exit door with key!");
                        var uiService = ServiceLocator.Instance.Get<IUIService>();
                        uiService?.ShowEndGamePanel(true);
                    }
                    else
                    {
                        Debug.Log("[Interaction] Need a key to open this door!");
                        // Có thể phát âm thanh "Locked" ở đây
                    }
                    return;
                }

            }

            // Check for lantern interaction
            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag("InteractableLantern"))
                {
                    var lantern = col.GetComponent<Game.Gameplay.Environment.LanternInteractable>();
                    if (lantern != null && itemComponent.HasLighter())
                    {
                        StartCoroutine(ToggleLanternRoutine(lantern));
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Auto exit when touching finish door with key (no button press needed).
        /// </summary>
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Finish") && itemComponent != null && itemComponent.KeyCount > 0)
            {
                Debug.Log("[Interaction] Auto-opening exit door!");
                var uiService = ServiceLocator.Instance.Get<IUIService>();
                uiService?.ShowEndGamePanel(true);
            }
        }

        /// <summary>
        /// Routine for toggling lantern with animation.
        /// </summary>
        private IEnumerator ToggleLanternRoutine(Game.Gameplay.Environment.LanternInteractable lantern)
        {
            var audioService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IAudioService>();
            audioService?.PlaySound("Fire");

            isInteracting = true;
            animationComponent.SetInteracting(true);

            yield return new WaitForSeconds(config.InteractionDelay);

            var uiService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IUIService>();
            uiService?.AddStep();

            lantern.ToggleLightOnly();

            yield return new WaitForSeconds(config.LanternInteractionCooldown);

            animationComponent.SetInteracting(false);
            yield return StartCoroutine(lantern.SafePathRoutine());
            isInteracting = false;
        }

        /// <summary>
        /// Routine for picking up items.
        /// </summary>
        private IEnumerator PickupRoutine(GameObject itemToPickup, string itemType)
        {
            isInteracting = true;

            var audioService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IAudioService>();
            audioService?.PlaySound("Pickup");

            animationComponent.SetInteracting(true);

            yield return new WaitForSeconds(config.InteractionDelay);

            if (itemToPickup != null)
            {
                switch (itemType)
                {
                    case "Lotus":
                        itemComponent.AddLotus();
                        break;
                    case "Lighter":
                        itemComponent.AddLighter();
                        break;
                    case "Key":
                        itemComponent.AddKey();
                        break;
                }

                Destroy(itemToPickup);
            }

            yield return new WaitForSeconds(config.InteractionDelay);

            var uiService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IUIService>();
            uiService?.AddStep();

            animationComponent.SetInteracting(false);
            isInteracting = false;
        }

        /// <summary>
        /// Stop all interaction coroutines.
        /// </summary>
        public void StopInteraction()
        {
            StopAllCoroutines();
            isInteracting = false;
            animationComponent.SetInteracting(false);
        }
    }
}
