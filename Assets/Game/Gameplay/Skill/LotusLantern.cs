using System.Collections;
using UnityEngine;

namespace Game.Gameplay.Skill

{
    public class LotusLantern : MonoBehaviour
    {

        [Header("Cài đặt bay")]
        public float flySpeed = 10f; // Tốc độ bay của hoa (ô/giây)
        public float maxLightDistance = 5f; // Quãng đường bay tối đa
        public float gridSize = 1f; // Kích thước ô gạch (nên để 1f)

        [Header("Cài đặt chướng ngại và đường đi")]
        public LayerMask obstacleLayer; // Lớp tường chắn
        public GameObject safePathPrefab; // Viên gạch sáng

        [Header("Hiệu ứng")]
        public float spawnDelay = 0.05f; // Độ trễ một chút sau khi đến ô mới (để hiệu ứng mượt)

        void Start() { }

        void Update() { }

        public void ActivateLantern(Vector2 facingDirection)
        {
            // Bắt đầu Coroutine bắt hoa bay và rải đường
            StartCoroutine(FlyAndSpawnTrailRoutine(facingDirection));
        }

        private IEnumerator FlyAndSpawnTrailRoutine(Vector2 facingDirection)
        {
            // 1. Raycast để tìm điểm cuối cùng (đập vào tường thì dừng)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, maxLightDistance, obstacleLayer);
            float actualDistance = maxLightDistance;

            if (hit.collider != null)
            {
                actualDistance = hit.distance;
                Debug.Log("Ánh sáng sẽ bị chặn bởi: " + hit.collider.gameObject.name);
            }

            // Tính số lượng gạch cần đẻ ra
            int pathTilesCount = Mathf.FloorToInt(actualDistance / gridSize);

            // Điểm bắt đầu (vị trí người chơi thả đèn)
            Vector3 startPos = transform.position;
            Vector3 currentTargetGridPos = startPos; // Vị trí ô gạch mà hoa đang hướng tới

            // 2. Chạy vòng lặp để bay từng ô một
            for (int i = 0; i < pathTilesCount; i++)
            {
                // Lưu lại vị trí ô gạch cũ (nơi hoa vừa rời đi) để đẻ đường ở đó
                Vector3 previousGridPos = currentTargetGridPos;

                // Tính toán vị trí ô gạch tiếp theo
                currentTargetGridPos = startPos + (Vector3)(facingDirection * (i + 1) * gridSize);

                // 3. Cho hoa bay mượt mà đến ô gạch tiếp theo
                // Khi khoảng cách còn xa, cứ di chuyển hoa về phía mục tiêu
                while (Vector3.Distance(transform.position, currentTargetGridPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, currentTargetGridPos, flySpeed * Time.deltaTime);
                    yield return null; // Chờ 1 khung hình rồi bay tiếp
                }

                // Khớp hoa vào đúng tọa độ ô gạch (cho chính xác)
                transform.position = currentTargetGridPos;

                // 4. "Đẻ" gạch sáng ở phía sau (tại ô gạch mà hoa vừa bay qua)
                // Quan trọng: Phải sinh ra làm con của SceneRoot (để null ở tham số thứ 4)
                // Nếu không, khi hoa bay tiếp, con đường sẽ bay theo hoa!
                Instantiate(safePathPrefab, previousGridPos, Quaternion.identity, null);

                // Nghỉ một chút trước khi bay sang ô tiếp theo (cho đẹp)
                yield return new WaitForSeconds(spawnDelay);
            }

            // 5. Sinh viên gạch cuối cùng ngay dưới chân bông hoa sau khi bay xong
            Instantiate(safePathPrefab, transform.position, Quaternion.identity, null);
        }
    }
}