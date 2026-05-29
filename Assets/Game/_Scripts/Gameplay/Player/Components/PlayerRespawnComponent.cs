using Game.Data;
using System.Collections;
using UnityEngine;

namespace Game.Gameplay.Player.Components
{
    /// <summary>
    /// Handles player respawn logic.
    /// </summary>
    public class PlayerRespawnComponent : MonoBehaviour
    {
        private Collider2D playerCollider;
        private Rigidbody2D rb;
        private GameConfig config;

        private Vector3 respawnPosition;
        private bool isRespawning = false;

        public bool IsRespawning => isRespawning;

        private void Start()
        {
            config = GameConfig.Instance;
            playerCollider = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
            SetRespawnPoint(transform.position);
        }

        /// <summary>
        /// Set a new respawn point.
        /// </summary>
        public void SetRespawnPoint(Vector3 newPoint)
        {
            respawnPosition = new Vector3(newPoint.x, newPoint.y, 0f);
        }

        /// <summary>
        /// Get the current respawn point.
        /// </summary>
        public Vector3 GetRespawnPoint()
        {
            return respawnPosition;
        }

        /// <summary>
        /// Trigger respawn sequence.
        /// </summary>
        public void Respawn()
        {
            if (!isRespawning)
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        /// <summary>
        /// Respawn routine with timing and effects.
        /// </summary>
        private IEnumerator RespawnRoutine()
        {
            isRespawning = true;

            // Disable collider to prevent interactions during respawn
            playerCollider.enabled = false;

            // Clear any velocity
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(config.RespawnColliderDisableDelay);

            // Move to respawn position
            Vector3 safePosition = new Vector3(respawnPosition.x, respawnPosition.y, 0f);
            transform.position = safePosition;

            yield return new WaitForSeconds(config.RespawnPositionSetDelay);

            // Re-enable collider
            playerCollider.enabled = true;
            isRespawning = false;
        }

        /// <summary>
        /// Force stop respawn routine.
        /// </summary>
        public void StopRespawn()
        {
            if (isRespawning)
            {
                StopAllCoroutines();
                playerCollider.enabled = true;
                isRespawning = false;
            }
        }
    }
}
