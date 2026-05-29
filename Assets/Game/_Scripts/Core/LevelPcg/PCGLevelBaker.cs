using Game.Core.Services;
using Game.Data;
using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Core.LevelPCG
{
    /// <summary>
    /// Procedural Content Generator for level baking.
    /// Generates levels using hill-climbing algorithm with seed-based reproducibility.
    /// Integrates with GameConfig and ServiceLocator for architecture compliance.
    /// </summary>
    public class PCGLevelBaker : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int numberOfMaps = 10;
        [SerializeField] private string baseFileName = "Map_";
        [SerializeField] private bool useSeeds = true;
        [SerializeField] private int startSeed = 12345;

        [Header("Dependencies")]
        [SerializeField] private LevelValidator validator;

        private GameConfig config;
        private TileType[,] virtualMap;
        private Vector2Int spawnPosition;

        private const string LEVELS_FOLDER = "Config/Levels";
        private const string JSON_EXTENSION = ".json";

        // Seeds for reproducibility
        private int currentSeed;

        private void Awake()
        {
            config = GameConfig.Instance;
            if (config == null)
            {
                Debug.LogError("GameConfig not found! PCG cannot proceed.");
                enabled = false;
                return;
            }

            if (validator == null)
            {
                Debug.LogError("LevelValidator not assigned! PCG cannot proceed.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Generate a single level with specific seed.
        /// </summary>
        public LevelData GenerateSingleLevel(int seed)
        {
            if (config == null) return null;

            currentSeed = seed;
            Random.InitState(seed);

            virtualMap = new TileType[config.LevelWidth, config.LevelHeight];
            InitializeRandomMap();

            int currentScore = validator.EvaluateVirtualMap(virtualMap, spawnPosition);
            int generations = 0;

            // Hill climbing evolution
            while (currentScore < 100 && generations < config.MaxGenerations)
            {
                generations++;
                TileType[,] mutatedMap = CloneMap(virtualMap);
                Mutate(mutatedMap);

                int newScore = validator.EvaluateVirtualMap(mutatedMap, spawnPosition);

                if (newScore >= currentScore)
                {
                    virtualMap = mutatedMap;
                    currentScore = newScore;
                }
            }

            // Export and return data
            if (currentScore >= 100)
            {
                LevelData levelData = ExportToLevelData(seed, generations);
                return levelData;
            }

            Debug.LogWarning($"Seed {seed} failed to generate valid map after {generations} iterations (Score: {currentScore})");
            return null;
        }

        /// <summary>
        /// Batch generate multiple levels and save to JSON.
        /// </summary>
        [ContextMenu("Bake Multiple Levels")]
        public void BakeMultipleLevels()
        {
            Debug.Log($"<color=cyan>━━━━ BẮT ĐẦU SẢN XUẤT {numberOfMaps} LEVELS ━━━━</color>");

            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < numberOfMaps; i++)
            {
                int seed = useSeeds ? (startSeed + i) : UnityEngine.Random.Range(0, 999999);
                string mapName = baseFileName + (i + 1).ToString("D2");

                LevelData levelData = GenerateSingleLevel(seed);

                if (levelData != null)
                {
                    SaveLevelToJSON(mapName, levelData, seed);
                    Debug.Log($"<color=green>✓ THÀNH CÔNG [{mapName}] - Seed: {seed}</color>");
                    successCount++;
                }
                else
                {
                    Debug.Log($"<color=red>✗ THẤT BẠI [{mapName}] - Seed: {seed}</color>");
                    failureCount++;
                }
            }

            Debug.Log($"<color=yellow>━━━━ KẾT QUẢ: {successCount} Thành công, {failureCount} Thất bại ━━━━</color>");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Generate a single level with validation and save to JSON.
        /// </summary>
        [ContextMenu("Bake Single Level")]
        public void BakeSingleLevel()
        {
            string mapName = baseFileName + "01";
            int seed = useSeeds ? startSeed : UnityEngine.Random.Range(0, 999999);

            LevelData levelData = GenerateSingleLevel(seed);

            if (levelData != null)
            {
                SaveLevelToJSON(mapName, levelData, seed);
                Debug.Log($"<color=green>✓ Level saved: {mapName}</color>");
            }
            else
            {
                Debug.LogError("Level generation failed!");
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Initialize map with random tiles based on configured probabilities.
        /// </summary>
        private void InitializeRandomMap()
        {
            virtualMap = new TileType[config.LevelWidth, config.LevelHeight];

            for (int x = 0; x < config.LevelWidth; x++)
            {
                for (int y = 0; y < config.LevelHeight; y++)
                {
                    int roll = Random.Range(0, 100);

                    if (roll < config.TileEmptyChance)
                        virtualMap[x, y] = TileType.Empty;
                    else if (roll < config.TileMiasmaChance)
                        virtualMap[x, y] = TileType.Miasma;
                    else if (roll < config.TileWallChance)
                        virtualMap[x, y] = TileType.Wall;
                    else if (roll < config.TileTableChance)
                        virtualMap[x, y] = TileType.Table;
                    else if (roll < config.TileLampChance)
                        virtualMap[x, y] = TileType.Lamp;
                    else
                        virtualMap[x, y] = TileType.Flower;
                }
            }

            // Place spawn zone at bottom
            int randomX = Random.Range(1, config.LevelWidth - 1);
            spawnPosition = new Vector2Int(randomX, 0);
            virtualMap[spawnPosition.x, spawnPosition.y] = TileType.Empty;
            virtualMap[randomX - 1, 0] = TileType.Empty;
            virtualMap[randomX + 1, 0] = TileType.Empty;

            // Place exit at top-right
            virtualMap[config.LevelWidth - 1, config.LevelHeight - 1] = TileType.ExitDoor;

            // Place key items dynamically (not hardcoded)
            PlaceKeyItem(TileType.Lighter, new Vector2Int(Random.Range(2, config.LevelWidth - 2), Random.Range(2, config.LevelHeight - 2)));
            PlaceKeyItem(TileType.Key, new Vector2Int(Random.Range(2, config.LevelWidth - 2), Random.Range(2, config.LevelHeight - 2)));
        }

        /// <summary>
        /// Place a key item at position if valid.
        /// </summary>
        private void PlaceKeyItem(TileType itemType, Vector2Int pos)
        {
            if (pos.x >= 0 && pos.x < config.LevelWidth && pos.y >= 0 && pos.y < config.LevelHeight)
            {
                if (virtualMap[pos.x, pos.y] == TileType.Empty)
                {
                    virtualMap[pos.x, pos.y] = itemType;
                }
            }
        }

        /// <summary>
        /// Clone map for mutation testing.
        /// </summary>
        private TileType[,] CloneMap(TileType[,] source)
        {
            TileType[,] clone = new TileType[config.LevelWidth, config.LevelHeight];
            System.Array.Copy(source, clone, source.Length);
            return clone;
        }

        /// <summary>
        /// Mutate random tile in map with weighted probabilities.
        /// </summary>
        private void Mutate(TileType[,] mapToMutate)
        {
            int rx = Random.Range(0, config.LevelWidth);
            int ry = Random.Range(0, config.LevelHeight);

            TileType currentTile = mapToMutate[rx, ry];

            // Protect key tiles
            if (currentTile == TileType.Lighter ||
                currentTile == TileType.Key ||
                currentTile == TileType.ExitDoor)
                return;

            // Protect spawn zone
            if (rx == spawnPosition.x && ry == spawnPosition.y)
                return;

            // Random tile replacement
            int roll = Random.Range(0, 100);

            if (roll < config.TileEmptyChance)
                mapToMutate[rx, ry] = TileType.Empty;
            else if (roll < config.TileMiasmaChance)
                mapToMutate[rx, ry] = TileType.Miasma;
            else if (roll < config.TileWallChance)
                mapToMutate[rx, ry] = TileType.Wall;
            else if (roll < config.TileTableChance)
                mapToMutate[rx, ry] = TileType.Table;
            else if (roll < config.TileLampChance)
                mapToMutate[rx, ry] = TileType.Lamp;
            else
                mapToMutate[rx, ry] = TileType.Flower;
        }

        /// <summary>
        /// Convert generated map to LevelData structure.
        /// </summary>
        private LevelData ExportToLevelData(int seed, int generationCount)
        {
            LevelData data = new LevelData
            {
                levelID = baseFileName + seed.ToString(),
                seed = seed,
                generationCount = generationCount,
                maxLight = config.MaxLampsPerLevel,
                spawnPos = spawnPosition,
                tiles = new System.Collections.Generic.List<TileData>()
            };

            // Export all non-empty tiles
            for (int x = 0; x < config.LevelWidth; x++)
            {
                for (int y = 0; y < config.LevelHeight; y++)
                {
                    TileType currentTile = virtualMap[x, y];

                    if (currentTile != TileType.Empty)
                    {
                        // For items placed on tables, export both
                        if (currentTile == TileType.Key ||
                            currentTile == TileType.Lighter ||
                            currentTile == TileType.Flower)
                        {
                            TileData tableTile = new TileData
                            {
                                type = TileType.Table.ToString(),
                                x = x,
                                y = y
                            };
                            data.tiles.Add(tableTile);
                        }

                        // Export the item/tile itself
                        TileData itemTile = new TileData
                        {
                            type = currentTile.ToString(),
                            x = x,
                            y = y
                        };
                        data.tiles.Add(itemTile);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Save level data to JSON file.
        /// </summary>
        private void SaveLevelToJSON(string fileName, LevelData levelData, int seed)
        {
            string folderPath = Path.Combine(Application.dataPath, "Game/Resources", LEVELS_FOLDER);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"Created directory: {folderPath}");
            }

            string json = JsonUtility.ToJson(levelData, true);
            string filePath = Path.Combine(folderPath, fileName + JSON_EXTENSION);

            File.WriteAllText(filePath, json);
            Debug.Log($"Saved: {filePath}");
        }

        /// <summary>
        /// Print map visualization to console for debugging.
        /// </summary>
        private void PrintMapToConsole()
        {
            string mapString = "=== BẢN ĐỒ PHÁT SINH ===\n";
            for (int y = config.LevelHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < config.LevelWidth; x++)
                {
                    if (x == spawnPosition.x && y == spawnPosition.y)
                        mapString += "🧍 ";
                    else
                        mapString += GetTileIcon(virtualMap[x, y]) + " ";
                }
                mapString += "\n";
            }
            Debug.Log(mapString);
        }

        /// <summary>
        /// Get emoji representation of tile for console display.
        /// </summary>
        private string GetTileIcon(TileType type)
        {
            return type switch
            {
                TileType.Empty => "⬜",
                TileType.Miasma => "🟪",
                TileType.Wall => "⬛",
                TileType.Table => "🟫",
                TileType.Flower => "🌸",
                TileType.Lighter => "🔥",
                TileType.Lamp => "🏮",
                TileType.Key => "🔑",
                TileType.ExitDoor => "🚪",
                _ => "❓"
            };
        }
    }

    /// <summary>
    /// Serializable level data structure for JSON export.
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public string levelID;
        public int seed;
        public int generationCount;
        public int maxLight;
        public int mapHeight;
        public Vector2Int spawnPos;
        public Vector2Int exitPos;
        public System.Collections.Generic.List<TileData> tiles = new System.Collections.Generic.List<TileData>();
        public System.Collections.Generic.List<TileData> safePaths = new System.Collections.Generic.List<TileData>();
    }

    /// <summary>
    /// Serializable tile data for JSON export.
    /// </summary>
    [System.Serializable]
    public class TileData
    {
        public string type;
        public int x;
        public int y;
        public int layer = 0;  // Layer/sorting order (from cell objects)
    }
}
