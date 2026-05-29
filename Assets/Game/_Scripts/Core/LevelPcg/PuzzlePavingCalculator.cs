using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.LevelPCG
{
    public class PuzzlePavingCalculator
    {
        public class PlacedItem
        {
            public Vector2Int Position;
            public char Type;
        }

        //  Đã thêm width và height vào tham số
        public List<PlacedItem> CalculateExactPlacements(List<Vector2Int> path, Vector2Int A, Vector2Int B, Vector2Int C, int width, int height)
        {
            List<PlacedItem> placements = new List<PlacedItem>();
            HashSet<Vector2Int> litTiles = new HashSet<Vector2Int>();
            HashSet<Vector2Int> occupiedSpots = new HashSet<Vector2Int> { A, B, C };

            litTiles.Add(A);

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2Int currentPos = path[i];
                Vector2Int targetPos = path[i + 1];

                if (!litTiles.Contains(targetPos))
                {
                    //  Truyền width, height xuống hàm kiểm tra
                    PlacedItem bestItem = FindBestCoverage(currentPos, path, i + 1, litTiles, occupiedSpots, width, height);

                    if (bestItem != null)
                    {
                        placements.Add(bestItem);
                        occupiedSpots.Add(bestItem.Position);

                        if (bestItem.Type == 'X') litTiles.UnionWith(GetX_Zone(currentPos, targetPos));
                        else if (bestItem.Type == 'Y') litTiles.UnionWith(GetY_Zone(bestItem.Position));
                    }
                    else
                    {
                        // 🚨 FIX 2 & 6: STRICT SOLVER - Bỏ "Dò dẫm", bóp chết map lỗi ngay lập tức
                        Debug.LogWarning($"[PuzzlePaving] Kẹt cứng tại {targetPos}. Reject toàn bộ map!");
                        return null; // Trả về null để Generator biết mà Re-roll
                    }
                }
            }
            return placements;
        }

        private PlacedItem FindBestCoverage(Vector2Int playerPos, List<Vector2Int> path, int targetPathIndex, HashSet<Vector2Int> litTiles, HashSet<Vector2Int> occupied, int width, int height)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            PlacedItem bestPlacement = null;
            int maxScore = -1;
            Vector2Int targetPos = path[targetPathIndex];

            foreach (var dir in dirs)
            {
                Vector2Int candidatePos = playerPos + dir;

                // BẢO VỆ MẢNG: Không cho phép đặt item ra ngoài ranh giới bản đồ!
                // Giới hạn x từ 1 đến width-2 (để né cột cửa Spawn/Exit). Giới hạn y từ 0 đến height-1.
                if (candidatePos.x <= 0 || candidatePos.x >= width - 1 || candidatePos.y < 0 || candidatePos.y >= height)
                    continue;

                // Vật phẩm không đè lên đường hoặc các điểm đặc biệt
                if (occupied.Contains(candidatePos) || path.Contains(candidatePos)) continue;

                // --- ĐÁNH GIÁ Y (Đèn 3x3) ---
                HashSet<Vector2Int> yZone = GetY_Zone(candidatePos);
                if (yZone.Contains(targetPos))
                {
                    int covY = CountPathCoverage(yZone, path, targetPathIndex, litTiles);
                    // 💡 Cân bằng mới: Khúc cua (>=2 ô) thì Đèn áp đảo (15 điểm/ô). Đường thẳng thì thua Hoa.
                    int scoreY = (covY >= 2) ? covY * 15 : 5;
                    if (scoreY > maxScore)
                    {
                        maxScore = scoreY;
                        bestPlacement = new PlacedItem { Position = candidatePos, Type = 'Y' };
                    }
                }

                // --- ĐÁNH GIÁ X (Bật Lửa / Hoa) ---
                HashSet<Vector2Int> xZone = GetX_Zone(playerPos, targetPos);
                if (xZone.Contains(targetPos))
                {
                    int covX = CountPathCoverage(xZone, path, targetPathIndex, litTiles);
                    // 💡 Cân bằng mới: Hoa là công cụ đi thẳng chuẩn mực (10 điểm/ô)
                    int scoreX = covX * 10;
                    if (scoreX > maxScore)
                    {
                        maxScore = scoreX;
                        bestPlacement = new PlacedItem { Position = candidatePos, Type = 'X' };
                    }
                }
            }
            return bestPlacement;
        }

        private int CountPathCoverage(HashSet<Vector2Int> zone, List<Vector2Int> path, int startIndex, HashSet<Vector2Int> litTiles)
        {
            int count = 0;
            for (int i = startIndex; i < path.Count; i++)
            {
                if (zone.Contains(path[i]) || litTiles.Contains(path[i])) count++;
                else break;
            }
            return count;
        }

        private HashSet<Vector2Int> GetY_Zone(Vector2Int yPos)
        {
            HashSet<Vector2Int> zone = new HashSet<Vector2Int>();
            // TRẢ LẠI 3x3 (9 Ô CHIẾU SÁNG)
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    zone.Add(new Vector2Int(yPos.x + x, yPos.y + y));
                }
            }
            return zone;
        }
        private HashSet<Vector2Int> GetX_Zone(Vector2Int playerPos, Vector2Int targetPos)
        {
            HashSet<Vector2Int> zone = new HashSet<Vector2Int>();
            Vector2Int direction = targetPos - playerPos; // Hướng người chơi đang tiến lên

            // SỬA LỖI: Chiếu sáng 3 ô liên tiếp dọc theo đường đi phía trước mặt
            zone.Add(targetPos);                   // Ô bước 1
            zone.Add(targetPos + direction);       // Ô bước 2
            zone.Add(targetPos + direction * 2);   // Ô bước 3

            return zone;
        }
    }
}
