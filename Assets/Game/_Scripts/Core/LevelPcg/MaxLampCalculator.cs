using Game.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.LevelPCG
{
    /// <summary>
    /// Calculates optimal maxLamp value for each generated map.
    /// Analyzes map difficulty and ensures balanced puzzle.
    /// </summary>
    public class MaxLampCalculator
    {
        private TileType[,] virtualMap;
        private int width;
        private int height;
        private Vector2Int spawnPos;
        private Vector2Int keyPos;
        private Vector2Int exitPos;

        private int totalLamps;
        private int totalFlowers;
        private List<Vector2Int> lampPositions;

        public MaxLampCalculator(TileType[,] map, Vector2Int spawn, Vector2Int key, Vector2Int exit)
        {
            virtualMap = map;
            width = map.GetLength(0);
            height = map.GetLength(1);
            spawnPos = spawn;
            keyPos = key;
            exitPos = exit;
            lampPositions = new List<Vector2Int>();

            AnalyzeMap();
        }

        // ⚠️ DEPRECATED: LevelMap constructor removed - LevelMap type no longer exists
        // Use the TileType[,] constructor instead
        /*
        public MaxLampCalculator(LevelMap map)
        {
            if (map == null)
            {
                Debug.LogError("MaxLampCalculator: LevelMap is null!");
                return;
            }

            // Convert cell-based map to TileType array
            virtualMap = map.ToTileArray();
            width = map.width;
            height = map.height;
            spawnPos = map.spawnPos;
            exitPos = map.exitPos;
            keyPos = FindPositionInMap(TileType.Key);
            lampPositions = new List<Vector2Int>();

            AnalyzeMap();
        }
        */

        /// <summary>
        /// Find first position of tile type in map.
        /// </summary>
        private Vector2Int FindPositionInMap(TileType tileType)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (virtualMap[x, y] == tileType)
                        return new Vector2Int(x, y);
                }
            }
            return Vector2Int.one * -1;
        }

        /// <summary>
        /// Analyze map and calculate optimal maxLamp value.
        /// </summary>
        public int CalculateMaxLamp()
        {
            Debug.Log($"[MaxLampCalculator] Analyzing map...");

            // 1. Tính toán độ dài và độ phức tạp
            int pathComplexity = CalculatePathComplexity();

            // 2. Base giới hạn là 3. Tăng lên dựa theo độ phức tạp của đường đi
            int baseMaxLamp = 3 + (pathComplexity / 2);

            // 3. Phạt thiếu Đèn: 
            // Nếu thuật toán A* không sinh ra cái đèn nào, người chơi phải phụ thuộc 100% vào Hoa.
            // Họ cần bình năng lượng to hơn để đi được xa hơn.
            if (totalLamps == 0)
            {
                baseMaxLamp += 2;
            }

            // 4. Cân bằng Hoa: Nếu có quá nhiều Hoa (> 10 bông), ta bóp maxLamp lại để giữ độ khó
            if (totalFlowers >= 10)
            {
                baseMaxLamp -= 1;
            }

            // Chốt chặn an toàn: Năng lượng ánh sáng luôn từ 3 đến 10
            int finalMaxLamp = Mathf.Clamp(baseMaxLamp, 3, 10);

            Debug.Log($"[MaxLampCalculator] Lighter/Flower Only? {(totalLamps == 0)} | Final MaxLamp: {finalMaxLamp}");

            return finalMaxLamp;
        }
        /// <summary>
        /// Analyze map layout and find all entities.
        /// </summary>
        private void AnalyzeMap()
        {
            lampPositions.Clear();
            totalLamps = 0;
            totalFlowers = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType tileType = virtualMap[x, y];

                    if (tileType == TileType.Lamp)
                    {
                        totalLamps++;
                        lampPositions.Add(new Vector2Int(x, y));
                    }
                    else if (tileType == TileType.Flower)
                    {
                        totalFlowers++;
                    }
                }
            }

            Debug.Log($"[MaxLampCalculator] Found {totalLamps} lamps and {totalFlowers} flowers");
        }

        /// <summary>
        /// Calculate path complexity based on distance and obstacles.
        /// </summary>
        private int CalculatePathComplexity()
        {
            // Estimate straight-line distances
            float spawnToKey = Vector2Int.Distance(spawnPos, keyPos);
            float keyToExit = Vector2Int.Distance(keyPos, exitPos);
            float totalDistance = spawnToKey + keyToExit;

            // Count obstacles between key points (rough estimate)
            int obstacleCount = CountObstaclesBetween(spawnPos, keyPos);
            obstacleCount += CountObstaclesBetween(keyPos, exitPos);

            // Complexity = distance + obstacles (normalized)
            int complexity = Mathf.Max(1, Mathf.RoundToInt((totalDistance + obstacleCount * 2) / 5f));

            Debug.Log($"[MaxLampCalculator] Path Complexity: {complexity} (distance: {totalDistance}, obstacles: {obstacleCount})");

            return complexity;
        }

        /// <summary>
        /// Rough count of obstacles between two points (for complexity estimation).
        /// </summary>
        private int CountObstaclesBetween(Vector2Int start, Vector2Int end)
        {
            int count = 0;
            int steps = Mathf.Max(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));

            for (int i = 0; i < steps; i++)
            {
                float t = steps > 0 ? (float)i / steps : 0;
                int x = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    TileType tileType = virtualMap[x, y];
                    if (tileType == TileType.Wall || tileType == TileType.Table || tileType == TileType.Miasma)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Calculate maxLamp with difficulty scaling (for future use with level numbers).
        /// </summary>
        public int CalculateMaxLampWithDifficulty(int levelNumber)
        {
            int baseMaxLamp = CalculateMaxLamp();

            // Scale difficulty every 10 levels
            int difficultyStage = levelNumber / 10;

            // Slowly increase maxLamp difficulty
            // Level 1-10: base
            // Level 11-20: base + 1
            // Level 21-30: base + 2
            // etc.
            int scaledMaxLamp = baseMaxLamp + (difficultyStage / 2);

            scaledMaxLamp = Mathf.Clamp(scaledMaxLamp, 2, 15);

            Debug.Log($"[MaxLampCalculator] Level {levelNumber}: Base={baseMaxLamp}, Scaled={scaledMaxLamp}");

            return scaledMaxLamp;
        }

        /// <summary>
        /// Get detailed analysis for debugging.
        /// </summary>
        public string GetAnalysisReport()
        {
            string report = $"=== Map Analysis Report ===\n";
            report += $"Spawn: {spawnPos}, Key: {keyPos}, Exit: {exitPos}\n";
            report += $"Total Lamps: {totalLamps}\n";
            report += $"Total Flowers: {totalFlowers}\n";
            report += $"Lamp Positions: {string.Join(", ", lampPositions)}\n";
            report += $"Calculated MaxLamp: {CalculateMaxLamp()}\n";
            return report;
        }
    }
}
