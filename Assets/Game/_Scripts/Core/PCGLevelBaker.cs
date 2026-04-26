using System.IO;
using UnityEngine;

namespace Game.Core
{
    public class PCGLevelBaker : MonoBehaviour
    {
        [Header("Thông số Lưới")]
        public int width = 13;
        public int height = 8;

        [Header("Trí Tuệ Nhân Tạo")]
        public LevelValidator validator;
        public int maxGenerations = 5000;

        [Header("Sản Xuất Hàng Loạt (Baking)")]
        public int numberOfMaps = 10;
        public string baseFileName = "Map_";

        private TileType[,] virtualMap;
        private Vector2Int spawnPosition;

        [ContextMenu("Chạy PCG")]
        public void RunEvolution()
        {
            if (validator == null) { Debug.LogError("Quên kéo LevelValidator vào kìa!"); return; }

            Debug.Log($"<color=cyan>BẮT ĐẦU SẢN XUẤT {numberOfMaps} MAPS...</color>");

            // --- VÒNG LẶP SẢN XUẤT ---
            for (int i = 1; i <= numberOfMaps; i++)
            {
                InitializeRandomMap();

                int currentScore = validator.EvaluateVirtualMap(virtualMap, spawnPosition);
                int generations = 0;

                // --- VÒNG LẶP LEO NÚI CHO TỪNG MAP ---
                while (currentScore < 100 && generations < maxGenerations)
                {
                    generations++;
                    TileType[,] clonedMap = CloneMap(virtualMap);
                    Mutate(clonedMap);

                    int newScore = validator.EvaluateVirtualMap(clonedMap, spawnPosition);

                    if (newScore >= currentScore)
                    {
                        virtualMap = clonedMap;
                        currentScore = newScore;
                    }
                }

                // --- KIỂM TRA KẾT QUẢ CỦA MAP HIỆN TẠI ---
                string mapName = baseFileName + i.ToString("D2"); // D2 giúp format số 1 thành "01", 2 thành "02"

                if (currentScore >= 100)
                {
                    Debug.Log($"<color=green>✓ THÀNH CÔNG [{mapName}]: Xong sau {generations} vòng.</color>");
                    ExportToJson(mapName); // Xuất file với tên có số thứ tự
                }
                else
                {
                    Debug.Log($"<color=red>X THẤT BẠI [{mapName}]: Bị kẹt sau {generations} vòng (Điểm: {currentScore}). Đang tạo lại...</color>");

                    // MẸO QUAN TRỌNG: Lùi i lại 1 bước để vòng lặp for chạy lại map này!
                    i--;
                }
            }

            Debug.Log($"<color=yellow>ĐÃ HOÀN TẤT SẢN XUẤT {numberOfMaps} MAPS!</color>");
        }

        private void InitializeRandomMap()
        {
            virtualMap = new TileType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int roll = Random.Range(0, 100);
                    if (roll < 45) virtualMap[x, y] = TileType.Miasma;
                    else if (roll < 65) virtualMap[x, y] = TileType.Empty;
                    else if (roll < 75) virtualMap[x, y] = TileType.Wall;
                    else if (roll < 85) virtualMap[x, y] = TileType.Table;
                    else if (roll < 95) virtualMap[x, y] = TileType.Lamp;
                    else virtualMap[x, y] = TileType.Flower;
                }
            }

            int randomX = Random.Range(0, width);
            spawnPosition = new Vector2Int(randomX, 0);
            virtualMap[spawnPosition.x, spawnPosition.y] = TileType.Empty;
            if (randomX > 0) virtualMap[randomX - 1, 0] = TileType.Empty;
            if (randomX < width - 1) virtualMap[randomX + 1, 0] = TileType.Empty;

            virtualMap[width - 1, height - 1] = TileType.ExitDoor;
            virtualMap[2, 5] = TileType.Lighter;
            virtualMap[10, 2] = TileType.Key;
        }

        private TileType[,] CloneMap(TileType[,] source)
        {
            TileType[,] clone = new TileType[width, height];
            System.Array.Copy(source, clone, source.Length);
            return clone;
        }

        private void Mutate(TileType[,] mapToMutate)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);

            TileType currentTile = mapToMutate[rx, ry];
            if (currentTile == TileType.Lighter || currentTile == TileType.Key || currentTile == TileType.ExitDoor) return;
            if (rx == spawnPosition.x && ry == spawnPosition.y) return;

            int roll = Random.Range(0, 100);
            if (roll < 30) mapToMutate[rx, ry] = TileType.Miasma;
            else if (roll < 55) mapToMutate[rx, ry] = TileType.Empty;
            else if (roll < 70) mapToMutate[rx, ry] = TileType.Wall;
            else if (roll < 85) mapToMutate[rx, ry] = TileType.Table;
            else if (roll < 95) mapToMutate[rx, ry] = TileType.Lamp;
            else mapToMutate[rx, ry] = TileType.Flower;
        }

        private void PrintMapToConsole()
        {
            string mapString = "=== BẢN ĐỒ KẾT QUẢ TIẾN HÓA ===\n";
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == spawnPosition.x && y == spawnPosition.y) mapString += "🧍 ";
                    else mapString += GetTileIcon(virtualMap[x, y]) + " ";
                }
                mapString += "\n";
            }
            Debug.Log(mapString);
        }

        private string GetTileIcon(TileType type)
        {
            switch (type)
            {
                case TileType.Empty: return "⬜";
                case TileType.Miasma: return "🟪";
                case TileType.Wall: return "⬛";
                case TileType.Table: return "🟫";
                case TileType.Flower: return "🌸";
                case TileType.Lighter: return "🔥";
                case TileType.Lamp: return "🏮";
                case TileType.Key: return "🔑";
                case TileType.ExitDoor: return "🚪";
                default: return "❓";
            }
        }

        private void ExportToJson(string fileName)
        {
            string folderPath = Application.dataPath + "/Game/_Scripts/Data/Levels";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            LevelData data = new LevelData();
            data.levelID = fileName;
            data.spawnPos = spawnPosition;
            data.maxLight = validator.maxLight;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType currentTile = virtualMap[x, y];

                    if (currentTile != TileType.Empty)
                    {
                        if (currentTile == TileType.Key || currentTile == TileType.Lighter || currentTile == TileType.Flower)
                        {
                            TileData tableTile = new TileData();
                            tableTile.type = TileType.Table.ToString();
                            tableTile.x = x;
                            tableTile.y = y;
                            data.tiles.Add(tableTile);
                        }

                        TileData itemTile = new TileData();
                        itemTile.type = currentTile.ToString();
                        itemTile.x = x;
                        itemTile.y = y;
                        data.tiles.Add(itemTile);
                    }
                }
            }

            string jsonText = JsonUtility.ToJson(data, true);
            string filePath = folderPath + "/" + fileName + ".json";

            File.WriteAllText(filePath, jsonText);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }

    [System.Serializable]
    public class TileData
    {
        public string type;
        public int x;
        public int y;
    }

    [System.Serializable]
    public class LevelData
    {
        public string levelID;
        public int maxLight;
        public Vector2Int spawnPos;
        public System.Collections.Generic.List<TileData> tiles = new System.Collections.Generic.List<TileData>();
    }
}