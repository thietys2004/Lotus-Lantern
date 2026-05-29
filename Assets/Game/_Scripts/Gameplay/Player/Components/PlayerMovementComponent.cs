using Game.Data;
using System.Collections;
using UnityEngine;

namespace Game.Gameplay.Player.Components
{
    /// <summary>
    /// Handles all player movement and grid-based pathfinding.
    /// Separated from PlayerController for better organization.
    /// </summary>
    public class PlayerMovementComponent : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        private GameConfig config;

        private bool isMoving = false;
        private bool isTurning = false;
        private float lastX = 0f;
        private float lastY = -1f;

        public bool IsMoving => isMoving;
        public bool IsTurning => isTurning;
        public float LastX => lastX;
        public float LastY => lastY;

        private void Start()
        {
            config = GameConfig.Instance;
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Process movement input and move the player if path is clear.
        /// </summary>
        public bool TryMove(float inputX, float inputY, bool canMove)
        {
            if (!canMove || isMoving || isTurning)
                return false;

            // Prevent diagonal movement
            if (inputX != 0)
                inputY = 0;

            if (inputX == 0 && inputY == 0)
                return false;

            // Check if direction changed
            if (inputX != lastX || inputY != lastY)
            {
                lastX = inputX;
                lastY = inputY;
                StartCoroutine(TurnCooldownRoutine());
                return true;
            }

            // Try to move in current direction
            Vector3 targetPos = transform.position + new Vector3(inputX, inputY, 0f) * config.GridSize;
            if (IsPathClear(targetPos))
            {
                StartCoroutine(MoveToGridRoutine(targetPos));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the path is clear of obstacles.
        /// </summary>
        private bool IsPathClear(Vector3 targetPos)
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                targetPos,
                config.PathClearBoxSize,
                0f
            );

            foreach (Collider2D col in hitColliders)
            {
                if (col.gameObject == gameObject) continue;
                if (col.isTrigger) continue;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if player is currently on a safe path.
        /// </summary>
        public bool IsSafePath()
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
                transform.position,
                config.SafePathCheckRadius
            );

            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag("SafePath"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Smooth grid-based movement with timeout protection.
        /// </summary>
        private IEnumerator MoveToGridRoutine(Vector3 targetPos)
        {
            isMoving = true;

            // Notify audio manager - will be replaced with service locator
            var audioService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IAudioService>();
            if (audioService != null)
                audioService.PlaySound("walking");
            else
                Debug.LogWarning("[PlayerMovement] IAudioService not registered - audio disabled");

            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (Vector3.Distance(transform.position, targetPos) > 0.01f && elapsed < config.MoveTimeout)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    config.MoveSpeed * Time.deltaTime
                );
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Timeout protection - revert to start position
            if (elapsed >= config.MoveTimeout)
            {
                transform.position = startPos;
            }
            else
            {
                transform.position = targetPos;

                // Notify UI about step count
                var uiService = Game.Core.Services.ServiceLocator.Instance.Get<Game.Core.Services.IUIService>();
                if (uiService != null)
                    uiService.AddStep();
                else
                    Debug.LogWarning("[PlayerMovement] IUIService not registered - UI updates disabled");
            }

            isMoving = false;

            // Stop audio
            if (audioService != null)
                audioService.PlaySound("stop");
        }

        /// <summary>
        /// Turn cooldown to prevent rapid direction changes.
        /// </summary>
        private IEnumerator TurnCooldownRoutine()
        {
            isTurning = true;
            yield return new WaitForSeconds(config.TurnDelay);
            isTurning = false;
        }

        /// <summary>
        /// Get current movement direction.
        /// </summary>
        public Vector2 GetCurrentDirection()
        {
            return new Vector2(lastX, lastY);
        }

        /// <summary>
        /// Stop all movement coroutines.
        /// </summary>
        public void StopMovement()
        {
            StopAllCoroutines();
            isMoving = false;
            isTurning = false;
        }
    }
}
