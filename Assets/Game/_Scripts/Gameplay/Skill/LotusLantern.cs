using System.Collections;
using UnityEngine;

namespace Game.Gameplay.Skill

{
    public class LotusLantern : MonoBehaviour
    {

        [Header("Cài đặt bay")]
        public float flySpeed = 10f;
        public float maxLightDistance = 5f;
        public float gridSize = 1f;

        [Header("Cài đặt chướng ngại và đường đi")]
        public LayerMask obstacleLayer;
        public GameObject safePathPrefab;

        [Header("Hiệu ứng")]
        public float spawnDelay = 0.05f;

        // public Transform SafePathContainer;
        void Start() { }

        void Update() { }

        public void ActivateLantern(Vector2 facingDirection)
        {

            StartCoroutine(FlyAndSpawnTrailRoutine(facingDirection));
        }

        private IEnumerator FlyAndSpawnTrailRoutine(Vector2 facingDirection)
        {
            // Raycast 
            RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, maxLightDistance, obstacleLayer);
            float actualDistance = maxLightDistance;

            if (hit.collider != null)
            {
                actualDistance = hit.distance;
            }


            int pathTilesCount = Mathf.FloorToInt(actualDistance / gridSize);


            Vector3 startPos = transform.position;
            Vector3 currentTargetGridPos = startPos;


            for (int i = 0; i < pathTilesCount; i++)
            {
                Vector3 previousGridPos = currentTargetGridPos;

                currentTargetGridPos = startPos + (Vector3)(facingDirection * (i + 1) * gridSize);
                if (IsWallAtPosition(currentTargetGridPos))
                {
                    break;
                }

                while (Vector3.Distance(transform.position, currentTargetGridPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, currentTargetGridPos, flySpeed * Time.deltaTime);
                    yield return null;
                }

                transform.position = currentTargetGridPos;

                SpawnTileIfEmpty(previousGridPos);

                yield return new WaitForSeconds(spawnDelay);
            }
            SpawnTileIfEmpty(transform.position);

            Destroy(gameObject, 0.1f);
        }
        private bool IsWallAtPosition(Vector3 position)
        {
            Collider2D col = Physics2D.OverlapCircle(position, 0.2f, obstacleLayer);

            if (col != null)
            {
                return true;
            }
            return false;
        }
        private void SpawnTileIfEmpty(Vector3 targetPos)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(targetPos, 0.4f);
            bool isAlreadyLit = false;

            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag("SafePath"))
                {
                    isAlreadyLit = true;
                    break;
                }
            }

            if (!isAlreadyLit)
            {

                GameObject tile = Instantiate(safePathPrefab, targetPos, Quaternion.identity);


                if (Game.Core.SoulFireManager.Instance != null && Game.Core.SoulFireManager.Instance.safePathContainer != null)
                {

                    tile.transform.SetParent(Game.Core.SoulFireManager.Instance.safePathContainer, true);
                }
                else
                {
                    Debug.LogWarning("Chưa gắn SafePathContainer cho SoulFireManager!");
                }
            }
        }
    }
}