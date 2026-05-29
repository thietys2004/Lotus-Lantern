using Game.Core.LevelPCG;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Game.Core
{
    /// <summary>
    /// Helper script to update JSON level files.
    /// Removes Wall tiles from tiles array and adds safePaths data.
    /// Run from Editor only.
    /// </summary>
    public class LevelJSONUpdater
    {
        [MenuItem("Tools/Game/Update Level JSONs - Remove Walls and Add SafePaths")]
        public static void UpdateAllLevelJSONs()
        {
            string levelsFolderPath = Path.Combine(Application.dataPath, "Game/Resources/Levels");
            
            if (!Directory.Exists(levelsFolderPath))
            {
                Debug.LogError($"Levels folder not found: {levelsFolderPath}");
                return;
            }

            string[] jsonFiles = Directory.GetFiles(levelsFolderPath, "Map_*.json");
            
            foreach (string jsonFile in jsonFiles)
            {
                UpdateLevelJSON(jsonFile);
            }

            Debug.Log($"<color=green>[LevelJSONUpdater] ✓ Updated {jsonFiles.Length} level JSON files</color>");
            AssetDatabase.Refresh();
        }

        private static void UpdateLevelJSON(string filePath)
        {
            try
            {
                string jsonText = File.ReadAllText(filePath);
                LevelData levelData = JsonUtility.FromJson<LevelData>(jsonText);

                // Remove Wall tiles from tiles array
                List<TileData> nonWallTiles = new List<TileData>();
                foreach (TileData tile in levelData.tiles)
                {
                    if (tile.type != nameof(TileType.Wall))
                    {
                        nonWallTiles.Add(tile);
                    }
                }
                levelData.tiles = nonWallTiles;

                // Add safePaths if not exists
                if (levelData.safePaths == null || levelData.safePaths.Count == 0)
                {
                    // Create 3 safe path tiles around spawn position
                    levelData.safePaths = new List<TileData>
                    {
                        new TileData { type = nameof(TileType.SafePath), x = levelData.spawnPos.x, y = levelData.spawnPos.y },
                        new TileData { type = nameof(TileType.SafePath), x = levelData.spawnPos.x - 1, y = levelData.spawnPos.y },
                        new TileData { type = nameof(TileType.SafePath), x = levelData.spawnPos.x + 1, y = levelData.spawnPos.y }
                    };
                }

                // Save updated JSON
                string updatedJson = JsonUtility.ToJson(levelData, true);
                File.WriteAllText(filePath, updatedJson);

                Debug.Log($"[LevelJSONUpdater] ✓ Updated: {Path.GetFileName(filePath)} - Removed {levelData.tiles.Count} non-wall tiles, added {levelData.safePaths.Count} safe paths");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LevelJSONUpdater] Error updating {filePath}: {ex.Message}");
            }
        }
    }
}
#endif
