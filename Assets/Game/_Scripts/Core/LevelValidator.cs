using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class LevelValidator : MonoBehaviour
    {
        public struct MapState
        {
            public Vector2Int Position;
            public int FlowerCount;
            public bool HasLighter;
            public bool HasKey;
            public int MiasmaShield;
            public int PickedFlowersMask;

            // --- THÊM MỚI: Biến theo dõi Đèn và Số bước ---
            public int ActiveLampsMask;
            public int StepCount;

            public string GetStateHash()
            {
                // Hash phải cộng thêm ActiveLampsMask để Bot biết trạng thái đèn
                return $"{Position.x},{Position.y}_{FlowerCount}_{HasLighter}_{HasKey}_{MiasmaShield}_{PickedFlowersMask}_{ActiveLampsMask}";
            }
        }

        [Header("Testing Parameters")]
        public int maxLight = 3; // Giới hạn ánh sáng tối đa
        public int initialFlowers = 0;
        public int miasmaStepsPerFlower = 3;

        // Hàm hỗ trợ quét tìm tọa độ của một loại vật thể bất kỳ (Hoa, Đèn...)
        private List<Vector2Int> GetPositions(TileType[,] virtualMap, int width, int height, TileType type)
        {
            List<Vector2Int> list = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (virtualMap[x, y] == type) list.Add(new Vector2Int(x, y));
                }
            }
            return list;
        }

        // --- HÀM CHẤM ĐIỂM (FITNESS FUNCTION) ĐÃ NÂNG CẤP ---
        public int EvaluateVirtualMap(TileType[,] virtualMap, Vector2Int startPos)
        {
            int width = virtualMap.GetLength(0);
            int height = virtualMap.GetLength(1);

            // Quét tìm danh sách Hoa và Đèn trước khi chạy
            List<Vector2Int> flowerPositions = GetPositions(virtualMap, width, height, TileType.Flower);
            List<Vector2Int> lampPositions = GetPositions(virtualMap, width, height, TileType.Lamp);

            Queue<MapState> queue = new Queue<MapState>();
            HashSet<string> visited = new HashSet<string>();

            MapState startState = new MapState
            {
                Position = startPos,
                FlowerCount = initialFlowers,
                HasLighter = false,
                HasKey = false,
                MiasmaShield = 0,
                PickedFlowersMask = 0,
                ActiveLampsMask = 0, // Bắt đầu chưa bật đèn nào
                StepCount = 0        // Bắt đầu ở bước 0
            };

            queue.Enqueue(startState);
            visited.Add(startState.GetStateHash());

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            int highestScore = 0;

            while (queue.Count > 0)
            {
                MapState currentState = queue.Dequeue();

                // 1. TÍNH ĐIỂM CƠ BẢN
                int currentScore = 0;
                if (currentState.HasLighter) currentScore += 30;
                if (currentState.HasKey) currentScore += 40;

                // --- LOGIC ĐẾM SỐ BƯỚC ---
                // Cộng thêm 1 điểm nhỏ cho mỗi 2 bước đi (Giúp AI thích đường ngoằn ngoèo hơn)
                int bonusStepScore = currentState.StepCount / 2;

                if (currentScore + bonusStepScore > highestScore)
                    highestScore = currentScore + bonusStepScore;

                // KIỂM TRA PHÁ ĐẢO (Vẫn giữ gốc 100 để vòng lặp Baker dừng lại)
                if (virtualMap[currentState.Position.x, currentState.Position.y] == TileType.ExitDoor && currentState.HasKey)
                {
                    // Trả về 100 + Điểm thưởng độ dài đoạn đường
                    return 100 + bonusStepScore;
                }

                // 2. NHẶT ĐỒ TRÊN BÀN (Lighter / Key)
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int adjPos = currentState.Position + dir;
                    if (adjPos.x >= 0 && adjPos.x < width && adjPos.y >= 0 && adjPos.y < height)
                    {
                        if (virtualMap[adjPos.x, adjPos.y] == TileType.Lighter) currentState.HasLighter = true;
                        if (virtualMap[adjPos.x, adjPos.y] == TileType.Key) currentState.HasKey = true;
                    }
                }

                // 3. DI CHUYỂN
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int nextPos = currentState.Position + dir;

                    if (nextPos.x < 0 || nextPos.x >= width || nextPos.y < 0 || nextPos.y >= height) continue;

                    TileType nextTile = virtualMap[nextPos.x, nextPos.y];

                    if (nextTile == TileType.Wall || nextTile == TileType.Table || nextTile == TileType.Lighter || nextTile == TileType.Key) continue;

                    MapState nextState = currentState;
                    nextState.Position = nextPos;
                    nextState.StepCount++; // <--- TĂNG BƯỚC ĐI TẠI ĐÂY

                    // --- LOGIC MAX LIGHT (ĐÈN) ---
                    if (nextTile == TileType.Lamp)
                    {
                        // Phải có Bật Lửa mới tính là bật được đèn
                        if (nextState.HasLighter)
                        {
                            int lampID = lampPositions.IndexOf(nextPos);

                            // Nếu đèn này chưa được bật trước đó
                            if (lampID != -1 && (nextState.ActiveLampsMask & (1 << lampID)) == 0)
                            {
                                // Đếm xem hiện tại đang có bao nhiêu cờ đèn được bật
                                int currentActiveCount = CountBits(nextState.ActiveLampsMask);

                                // NẾU VƯỢT QUÁ MAX LIGHT -> Báo lỗi nhánh này, không cho đi tiếp!
                                if (currentActiveCount >= maxLight)
                                {
                                    continue;
                                }

                                // Nếu còn slot, bật đèn này lên
                                nextState.ActiveLampsMask |= (1 << lampID);
                            }
                        }
                    }

                    // --- LOGIC NHẶT HOA ---
                    if (nextTile == TileType.Flower)
                    {
                        int flowerID = flowerPositions.IndexOf(nextPos);
                        if ((nextState.PickedFlowersMask & (1 << flowerID)) == 0)
                        {
                            nextState.FlowerCount++;
                            nextState.PickedFlowersMask |= (1 << flowerID);
                        }
                    }

                    // --- LOGIC CHƯỚNG KHÍ ---
                    if (nextTile == TileType.Miasma)
                    {
                        if (nextState.MiasmaShield > 0) nextState.MiasmaShield--;
                        else if (nextState.FlowerCount > 0)
                        {
                            nextState.FlowerCount--;
                            nextState.MiasmaShield = miasmaStepsPerFlower - 1;
                        }
                        else continue; // Kẹt chướng khí
                    }
                    else
                    {
                        nextState.MiasmaShield = 0;
                    }

                    string stateHash = nextState.GetStateHash();
                    if (!visited.Contains(stateHash))
                    {
                        visited.Add(stateHash);
                        queue.Enqueue(nextState);
                    }
                }
            }

            return highestScore;
        }

        // Hàm phụ trợ để đếm số lượng bit 1 (Đếm số đèn đang bật)
        private int CountBits(int n)
        {
            int count = 0;
            while (n > 0)
            {
                count += n & 1;
                n >>= 1;
            }
            return count;
        }
    }
}