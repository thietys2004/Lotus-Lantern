using Game.Core.LevelPCG;
using Game.Data;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Random = UnityEngine.Random;

namespace Game.Core.LevelPCG
{
    /// <summary>
    /// 🚀 COMPLETE PCG PIPELINE v2.0 - DETERMINISTIC + SOLVER-BASED
    /// 
    /// STEP 1: Path Generation (A → C → B, deterministic)
    /// STEP 2: Path Validation (check bounds, no overlaps)
    /// STEP 3: Puzzle Placement (X/Y via PuzzlePavingCalculator)
    /// STEP 4: Map Finalization (tables, flowers, decorative obstacles)
    /// STEP 5: Export to JSON
    /// </summary>
    public class CompletePCGImplementationGuide : MonoBehaviour
    {
        [Header("PCG Settings")]
        [SerializeField] private int numberOfMapsToGenerate = 5;
        [SerializeField] private int startSeed = 10000;

        private GameConfig config;
        private List<GeneratedMapData> generatedMaps = new List<GeneratedMapData>();

        public class GeneratedMapData
        {
            public int Seed { get; set; }
            public TileType[,] Map { get; set; }
            public TileType[,] Items { get; set; }
            public Vector2Int SpawnPos { get; set; }
            public Vector2Int ExitPos { get; set; }
            public Vector2Int KeyPos { get; set; }
            public HashSet<Vector2Int> ProtectedPath { get; set; }
            public float Difficulty { get; set; }
            public int FlowerCount { get; set; }
            public int LighterCount { get; set; }
            public int KeyCount { get; set; }
            public int MaxLamp { get; set; }
        }

        private void Awake()
        {
            config = GameConfig.Instance;
            generatedMaps = new List<GeneratedMapData>();
        }


        [ContextMenu("STEP Generate Map ")]
        public void Step5_CompleteHybridPipeline()
        {
            generatedMaps.Clear();
            int width = 13;
            int height = 8;

            int successfullyGenerated = 0;
            int currentSeed = startSeed;
            int maxRetries = 100; // Giới hạn số lần thử lại để chống treo máy
            int retries = 0;

            LevelValidator validator = GetComponent<LevelValidator>();

            // 💡 SỬA LỖI 1: Vòng lặp WHILE. Chỉ dừng khi tạo ĐỦ số map thành công!
            while (successfullyGenerated < numberOfMapsToGenerate && retries < maxRetries)
            {
                Random.InitState(currentSeed);

                TileType[,] map = new TileType[width, height];
                TileType[,] items = new TileType[width, height];
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        map[x, y] = TileType.Empty;
                        items[x, y] = TileType.Empty;
                    }

                // ========== 1. XÁC ĐỊNH SPAWN, EXIT VÀ KEY ==========
                int spawnY = Random.Range(2, height - 2);
                Vector2Int spawnPos = new Vector2Int(0, spawnY);
                Vector2Int exitPos = new Vector2Int(width - 1, Random.Range(1, height - 1));
                Vector2Int keyPos = GenerateKeyPositionOutsideAxis(spawnPos, exitPos, width, height);

                // ========== 2. ĐÀO ĐƯỜNG ĐI TRƯỚC (PATH CARVING) ==========
                List<Vector2Int> mainPath = BuildPathThroughC(spawnPos, keyPos, exitPos, width, height);

                HashSet<Vector2Int> protectedPath = new HashSet<Vector2Int>(mainPath);
                protectedPath.Add(new Vector2Int(0, spawnY - 1));
                protectedPath.Add(new Vector2Int(0, spawnY + 1));

                // ========== 3. TÌM VỊ TRÍ ĐẶT LIGHTER AN TOÀN ==========
                Vector2Int[] preferredLighterSpots = {
                    new Vector2Int(0, spawnY - 2), new Vector2Int(0, spawnY + 2),
                    new Vector2Int(1, spawnY + 1), new Vector2Int(1, spawnY), new Vector2Int(1, spawnY - 1)
                };

                List<Vector2Int> validLighterSpots = new List<Vector2Int>();
                foreach (var spot in preferredLighterSpots)
                {
                    if (spot.x >= 0 && spot.x < width && spot.y >= 0 && spot.y < height && !protectedPath.Contains(spot))
                        validLighterSpots.Add(spot);
                }

                Vector2Int lighterPos = spawnPos;
                if (validLighterSpots.Count > 0)
                {
                    lighterPos = validLighterSpots[Random.Range(0, validLighterSpots.Count)];
                }
                else
                {
                    // 🚨 FIX 7: KHÔNG TÌM ĐƯỢC CHỖ ĐẶT LIGHTER -> REJECT MAP NGAY!
                    Debug.LogWarning($"[PCG] Không tìm được chỗ an toàn cho Lighter. Re-roll Seed {currentSeed}!");
                    currentSeed++; retries++; continue;
                }

                map[lighterPos.x, lighterPos.y] = TileType.Table;
                items[lighterPos.x, lighterPos.y] = TileType.Lighter;

                HashSet<Vector2Int> occupiedByPuzzle = new HashSet<Vector2Int> { lighterPos, keyPos };

                // ========== 4. PUZZLE PLACEMENT (RẢI HOA VÀ ĐÈN DỌC ĐƯỜNG) ==========
                PuzzlePavingCalculator pavingCalc = new PuzzlePavingCalculator();
                List<PuzzlePavingCalculator.PlacedItem> placements = pavingCalc.CalculateExactPlacements(
                    mainPath, spawnPos, exitPos, keyPos, width, height
                );

                // 🚨 FIX 2 & 6: BẮT NGAY LỖI TỪ GREEDY SOLVER
                if (placements == null)
                {
                    Debug.LogWarning($"[PCG] Greedy Solver không tìm được lời giải. Đập đi xây lại Seed {currentSeed}!");
                    currentSeed++; retries++; continue; // Bỏ map này, sang seed mới lập tức!
                }

                map[exitPos.x, exitPos.y] = TileType.ExitDoor;
                items[keyPos.x, keyPos.y] = TileType.Key;
                map[keyPos.x, keyPos.y] = TileType.Table;

                int lampCount = 0;
                int flowerCount = 0;

                foreach (var placement in placements)
                {
                    if (occupiedByPuzzle.Contains(placement.Position)) continue;
                    occupiedByPuzzle.Add(placement.Position);

                    if (placement.Type == 'X')
                    {
                        items[placement.Position.x, placement.Position.y] = TileType.Flower;
                        map[placement.Position.x, placement.Position.y] = TileType.Table;
                        flowerCount++;
                    }
                    else if (placement.Type == 'Y')
                    {
                        map[placement.Position.x, placement.Position.y] = TileType.Lamp;
                        items[placement.Position.x, placement.Position.y] = TileType.Empty;
                        lampCount++;
                    }
                }

                // ========== 5. RẢI VẬT CẢN VÀ HOA DỰ PHÒNG ==========
                float difficulty = CalculateWaveDifficulty(successfullyGenerated); // Dùng số map thành công làm độ khó
                FillDecorativeObstacles(map, items, protectedPath, spawnPos, width, height, difficulty, occupiedByPuzzle);

                int randomFlowerCount;
                PlaceFlowersInDecorativeObstacles(items, map, protectedPath, spawnPos, exitPos, keyPos, occupiedByPuzzle, difficulty, out randomFlowerCount);
                flowerCount += randomFlowerCount;

                // 💡 FIX "BÀN TÀNG HÌNH": TẠO MẢNG GỘP VỚI ƯU TIÊN TABLE
                // ✅ Table (vật cản vật lý) LUÔN được ưu tiên, Items (trên bàn) KHÔNG ghi đè
                TileType[,] combinedMap = new TileType[width, height];
                for (int cx = 0; cx < width; cx++)
                {
                    for (int cy = 0; cy < height; cy++)
                    {
                        combinedMap[cx, cy] = map[cx, cy];  // Map có Tables, Walls
                        // ✅ Items chỉ được thêm nếu vị trí KHÔNG phải Table (vật cản vật lý)
                        if (items[cx, cy] != TileType.Empty && map[cx, cy] != TileType.Table)
                            combinedMap[cx, cy] = items[cx, cy];
                    }
                }

                // Giao combinedMap (Có đủ Hoa, Đèn, Tường) cho MaxLampCalculator
                MaxLampCalculator lampCalc = new MaxLampCalculator(combinedMap, spawnPos, keyPos, exitPos);
                int maxLamp = lampCalc.CalculateMaxLamp();

                // ========== 6. CHẠY LEVEL VALIDATOR ĐỂ NGHIỆM THU ==========
                bool isPlayable = true;

                // 1. Convert dữ liệu
                var testMapData = ConvertToMapData(map, items, spawnPos, exitPos, keyPos);

                // 2. Nạp vào Ground Truth Validator
                var groundTruthValidator = new MapValidatorTester.MapValidator(testMapData);

                // 3. Tìm đường
                var path = groundTruthValidator.FindPath();

                if (path == null)
                {
                    isPlayable = false; // Kẹt đường -> Re-roll!
                }

                // 💡 SỬA LỖI 3: NẾU MAP OK THÌ MỚI LƯU. NẾU LỖI THÌ NÉM VÀO SỌT RÁC VÀ TĂNG SEED!
                if (isPlayable)
                {
                    generatedMaps.Add(new GeneratedMapData
                    {
                        Seed = currentSeed,
                        Map = map,
                        Items = items,
                        SpawnPos = spawnPos,
                        ExitPos = exitPos,
                        KeyPos = keyPos,
                        ProtectedPath = protectedPath,
                        Difficulty = difficulty,
                        FlowerCount = flowerCount,
                        LighterCount = 1,
                        KeyCount = 1,
                        MaxLamp = maxLamp
                    });

                    Debug.Log($"<color=green>✓ Map {currentSeed} THÀNH CÔNG! Lighter Pos: {lighterPos} | Lamps: {lampCount}</color>");
                    successfullyGenerated++; // Tăng tiến độ
                }
                else
                {
                    Debug.LogWarning($"<color=orange>⚠ Map {currentSeed} Bị Kẹt (Vô nghiệm). Hủy map và Re-roll seed mới...</color>");
                }

                currentSeed++; // Đổi seed khác cho lần tạo tiếp theo
                retries++;
            }

            if (retries >= maxRetries)
            {
                Debug.LogError($"[PCG] Đã đạt giới hạn Re-roll an toàn ({maxRetries} lần). Chỉ tạo được {successfullyGenerated}/{numberOfMapsToGenerate} map.");
            }

            // Mọi map trong generatedMaps lúc này đều được bảo kê 100% chơi được
            foreach (var mapData in generatedMaps) ExportMapToJSON(mapData);
        }
        private Vector2Int GenerateKeyPositionOutsideAxis(Vector2Int spawn, Vector2Int exit, int width, int height)
        {
            Vector2Int keyPos;
            int attempts = 0;

            do
            {
                keyPos = new Vector2Int(Random.Range(2, width - 2), Random.Range(1, height - 1));
                attempts++;
            } while ((Mathf.Abs(keyPos.x - spawn.x) < 3 || Mathf.Abs(keyPos.x - exit.x) < 3) && attempts < 50);

            return keyPos;
        }

        /// <summary>
        /// STEP 1: Build deterministic path A → (neighbor of C) → B
        /// Guarantees path goes through a cell adjacent to key position
        /// </summary>
        private List<Vector2Int> BuildPathThroughC(Vector2Int A, Vector2Int C, Vector2Int B, int width, int height)
        {
            List<Vector2Int> path = new List<Vector2Int>();

            // Find neighbor of C closest to A
            Vector2Int nearC = GetNeighborOfC(C, A, width, height);

            // Build A → nearC
            path.AddRange(BuildStraightPath(A, nearC));

            // Ensure nearC is in path
            if (!path.Contains(nearC))
                path.Add(nearC);

            // Build nearC → B
            path.AddRange(BuildStraightPath(nearC, B));

            return path;
        }

        /// <summary>
        /// Build straight line path from source to destination
        /// </summary>
        private List<Vector2Int> BuildStraightPath(Vector2Int from, Vector2Int to)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            int x = from.x;
            int y = from.y;

            while (x != to.x || y != to.y)
            {
                if (x != to.x)
                    x += (to.x > x) ? 1 : -1;
                else
                    y += (to.y > y) ? 1 : -1;

                path.Add(new Vector2Int(x, y));
            }

            return path;
        }

        /// <summary>
        /// Get the neighbor of C that is closest to 'from'
        /// This ensures path passes through C's vicinity
        /// </summary>
        private Vector2Int GetNeighborOfC(Vector2Int C, Vector2Int from, int width, int height)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>
            {
                C + Vector2Int.up,
                C + Vector2Int.down,
                C + Vector2Int.left,
                C + Vector2Int.right
            };

            // 🚨 FIX 3: CHẶN OUT-OF-BOUNDS
            neighbors = neighbors.FindAll(n => n.x >= 0 && n.x < width && n.y >= 0 && n.y < height);

            if (neighbors.Count == 0) return C; // Fallback an toàn

            // Sort by Manhattan distance to 'from'
            neighbors.Sort((a, b) =>
                ManhattanDistance(a, from).CompareTo(ManhattanDistance(b, from))
            );

            return neighbors[0];
        }

        /// <summary>
        /// STEP 2: Validate path
        /// - No out-of-bounds
        /// - No duplicates
        /// - Continuous connectivity
        /// </summary>
        private bool ValidatePath(List<Vector2Int> path, int width, int height)
        {
            if (path == null || path.Count == 0)
            {
                Debug.LogError("[PCG] Path is empty");
                return false;
            }

            // Check bounds
            foreach (var pos in path)
            {
                if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
                {
                    Debug.LogError($"[PCG] Path out of bounds at {pos}");
                    return false;
                }
            }

            // Check duplicates
            HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
            foreach (var pos in path)
            {
                if (seen.Contains(pos))
                {
                    Debug.LogError($"[PCG] Duplicate position in path: {pos}");
                    return false;
                }
                seen.Add(pos);
            }

            return true;
        }

        /// <summary>
        /// Calculate difficulty wave (15 map cycle: 0→1→0)
        /// </summary>
        private float CalculateWaveDifficulty(int mapIndex)
        {
            int cycleIndex = mapIndex % 15;

            if (cycleIndex < 7)
                return cycleIndex / 7f;
            else if (cycleIndex < 15)
                return (14 - cycleIndex) / 7f;

            return 0.5f;
        }

        /// <summary>
        /// Fill decorative obstacles (tables) in empty spaces
        /// Respects path, safe zone, and occupied spots
        /// </summary>
        private void FillDecorativeObstacles(TileType[,] map, TileType[,] items, HashSet<Vector2Int> path,
            Vector2Int spawnPos, int width, int height, float difficulty, HashSet<Vector2Int> occupiedByPuzzle)
        {
            int tableChance = (int)(20 + (difficulty * 20));

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    bool onPath = path.Contains(pos);
                    bool inSafeZone = (x == spawnPos.x && Mathf.Abs(y - spawnPos.y) <= 1);
                    bool isPuzzleSpot = occupiedByPuzzle.Contains(pos);

                    if (!onPath && !inSafeZone && !isPuzzleSpot && items[x, y] == TileType.Empty && map[x, y] == TileType.Empty)
                    {
                        if (Random.Range(0, 100) < tableChance)
                            map[x, y] = TileType.Table;
                    }
                }
            }
        }

        /// <summary>
        /// Place flowers randomly on available decorative tables
        /// </summary>
        private void PlaceFlowersInDecorativeObstacles(TileType[,] items, TileType[,] map, HashSet<Vector2Int> path,
            Vector2Int spawn, Vector2Int exit, Vector2Int keyPos, HashSet<Vector2Int> occupiedByPuzzle,
            float difficulty, out int flowerCount)
        {
            flowerCount = 0;
            int width = items.GetLength(0);
            int height = items.GetLength(1);
            List<Vector2Int> availableTables = new List<Vector2Int>();

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (map[x, y] == TileType.Table && items[x, y] == TileType.Empty &&
                        !occupiedByPuzzle.Contains(pos) && pos != keyPos)
                    {
                        availableTables.Add(pos);
                    }
                }
            }

            int baseFlowersNeeded = Mathf.Max(5, path.Count / 3);
            int totalFlowersNeeded = Mathf.RoundToInt(baseFlowersNeeded * Mathf.Lerp(1.5f, 0.8f, difficulty));
            totalFlowersNeeded = Mathf.Clamp(totalFlowersNeeded, 5, availableTables.Count);

            for (int i = 0; i < totalFlowersNeeded && availableTables.Count > 0; i++)
            {
                int rIdx = Random.Range(0, availableTables.Count);
                items[availableTables[rIdx].x, availableTables[rIdx].y] = TileType.Flower;
                availableTables.RemoveAt(rIdx);
                flowerCount++;
            }
        }

        /// <summary>
        /// Calculate Manhattan distance
        /// </summary>
        private int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// STEP 5: Export map to JSON format
        /// </summary>
        private void ExportMapToJSON(GeneratedMapData mapData)
        {
            int width = mapData.Map.GetLength(0);
            int height = mapData.Map.GetLength(1);
            List<string> cellList = new List<string>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    List<string> objectsInCell = new List<string>();

                    objectsInCell.Add("{\"type\":\"ground\",\"layer\":-1}");
                    objectsInCell.Add("{\"type\":\"hazard\",\"layer\":-2}");

                    bool isSafeZone = (x == mapData.SpawnPos.x && Mathf.Abs(y - mapData.SpawnPos.y) <= 1);
                    if (isSafeZone)
                        objectsInCell.Add("{\"type\":\"safe_path\",\"layer\":1}");

                    TileType mapTile = mapData.Map[x, y];
                    TileType itemTile = mapData.Items[x, y];

                    bool needsTable = (mapTile == TileType.Table) ||
                                      (itemTile == TileType.Flower || itemTile == TileType.Key || itemTile == TileType.Lighter);
                    if (needsTable)
                        objectsInCell.Add("{\"type\":\"table\",\"layer\":0}");

                    if (mapTile == TileType.ExitDoor)
                        objectsInCell.Add("{\"type\":\"exit\",\"layer\":2}");
                    else if (mapTile == TileType.Lamp)
                        objectsInCell.Add("{\"type\":\"lamp\",\"layer\":2}");

                    if (itemTile == TileType.Flower)
                        objectsInCell.Add("{\"type\":\"flower\",\"layer\":2}");
                    else if (itemTile == TileType.Key)
                        objectsInCell.Add("{\"type\":\"key\",\"layer\":2}");
                    else if (itemTile == TileType.Lighter)
                        objectsInCell.Add("{\"type\":\"lighter\",\"layer\":2}");

                    string cellJson = $"{{\"x\":{x},\"y\":{y},\"objects\":[{string.Join(",", objectsInCell)}]}}";
                    cellList.Add(cellJson);
                }
            }

            List<string> pathCoords = new List<string>();
            foreach (var pos in mapData.ProtectedPath)
                pathCoords.Add($"{{\"x\":{pos.x},\"y\":{pos.y}}}");
            string pathJson = "[" + string.Join(",", pathCoords) + "]";

            string itemsJson = $@"{{
    ""flowers"":{mapData.FlowerCount},
    ""lighter"":{mapData.LighterCount},
    ""key"":{mapData.KeyCount}
  }}";

            string cellsJson = "[" + string.Join(",", cellList) + "]";
            string json = $@"{{
  ""levelID"":""Map_{mapData.Seed}"",
  ""seed"":{mapData.Seed},
  ""generationCount"":1,
  ""maxLight"":{mapData.MaxLamp},
  ""difficulty"":{mapData.Difficulty:F2},
  ""width"":{width},
  ""height"":{height},
  ""spawnPos"":{{""x"":{mapData.SpawnPos.x},""y"":{mapData.SpawnPos.y}}},
  ""exitPos"":{{""x"":{mapData.ExitPos.x},""y"":{mapData.ExitPos.y}}},
  ""keyPos"":{{""x"":{mapData.KeyPos.x},""y"":{mapData.KeyPos.y}}},
  ""pathTracking"":{pathJson},
  ""itemsInventory"":{itemsJson},
  ""cells"":{cellsJson}
}}";

            string path = System.IO.Path.Combine(Application.dataPath, "Game", "Resources", "Levels");
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(System.IO.Path.Combine(path, $"Map_{mapData.Seed:D5}.json"), json);
            Debug.Log($"[PCG] JSON exported: Map_{mapData.Seed:D5}.json");
        }

        // 💡 ADAPTER PATTERN: Chuyển đổi dữ liệu thô thành chuẩn MapData của Tester
        private MapValidatorTester.MapData ConvertToMapData(TileType[,] baseMap, TileType[,] itemsMap, Vector2Int spawnPos, Vector2Int exitPos, Vector2Int keyPos)
        {
            int width = baseMap.GetLength(0);
            int height = baseMap.GetLength(1);

            var mapData = new MapValidatorTester.MapData
            {
                width = width,
                height = height,
                spawnPos = new MapValidatorTester.Position { x = spawnPos.x, y = spawnPos.y },
                exitPos = new MapValidatorTester.Position { x = exitPos.x, y = exitPos.y },
                keyPos = new MapValidatorTester.Position { x = keyPos.x, y = keyPos.y },
                cells = new List<MapValidatorTester.Cell>()
            };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new MapValidatorTester.Cell { x = x, y = y, objects = new List<MapValidatorTester.ObjectData>() };

                    // 1. Quét Base Layer
                    TileType baseTile = baseMap[x, y];
                    if (baseTile == TileType.Wall) cell.objects.Add(new MapValidatorTester.ObjectData { type = "wall" });
                    if (baseTile == TileType.Table) cell.objects.Add(new MapValidatorTester.ObjectData { type = "table" });
                    if (baseTile == TileType.Lamp) cell.objects.Add(new MapValidatorTester.ObjectData { type = "lamp" });
                    if (baseTile == TileType.ExitDoor) cell.objects.Add(new MapValidatorTester.ObjectData { type = "exit" });
                    if (baseTile == TileType.Miasma) cell.objects.Add(new MapValidatorTester.ObjectData { type = "hazard" });

                    // Thêm Safe Path chuẩn 1x3
                    if (x == spawnPos.x && Mathf.Abs(y - spawnPos.y) <= 1)
                        cell.objects.Add(new MapValidatorTester.ObjectData { type = "safe_path" });

                    // 2. Quét Item Layer
                    TileType itemTile = itemsMap[x, y];
                    if (itemTile == TileType.Flower) cell.objects.Add(new MapValidatorTester.ObjectData { type = "flower" });
                    if (itemTile == TileType.Lighter) cell.objects.Add(new MapValidatorTester.ObjectData { type = "lighter" });
                    if (itemTile == TileType.Key) cell.objects.Add(new MapValidatorTester.ObjectData { type = "key" });

                    mapData.cells.Add(cell);
                }
            }
            return mapData;
        }
    }
}