using Game.Data;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.LevelPCG
{
    // 🚀 SINGLE SOURCE OF TRUTH WRAPPER
    // File này đóng vai trò Cầu nối (Adapter) cho các script cũ.
    // Toàn bộ logic kiểm định giờ đây dựa 100% vào AI của MapValidatorTester!
    public class LevelValidator : MonoBehaviour
    {
        // 1. Hàm tương thích cho PCGLevelBaker (Dùng mảng gộp)
        public int EvaluateVirtualMap(TileType[,] combinedMap, Vector2Int startPos)
        {
            if (combinedMap == null) return 0;
            int width = combinedMap.GetLength(0);
            int height = combinedMap.GetLength(1);

            TileType[,] baseMap = new TileType[width, height];
            TileType[,] itemsMap = new TileType[width, height];
            Vector2Int exitPos = Vector2Int.one * -1;
            Vector2Int keyPos = Vector2Int.one * -1;

            // Dịch mảng cũ sang mảng 2 layer
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType tile = combinedMap[x, y];

                    if (tile == TileType.ExitDoor) exitPos = new Vector2Int(x, y);
                    if (tile == TileType.Key) keyPos = new Vector2Int(x, y);

                    // Lót bàn gỗ bên dưới đồ vật
                    if (tile == TileType.Flower || tile == TileType.Lighter || tile == TileType.Key)
                    {
                        itemsMap[x, y] = tile;
                        baseMap[x, y] = TileType.Table;
                    }
                    else
                    {
                        baseMap[x, y] = tile;
                        itemsMap[x, y] = TileType.Empty;
                    }
                }
            }

            if (exitPos == Vector2Int.one * -1 || keyPos == Vector2Int.one * -1) return 0;

            // Gọi MapValidatorTester để quyết định
            var mapData = ConvertToMapData(baseMap, itemsMap, startPos, exitPos, keyPos);
            var validator = new MapValidatorTester.MapValidator(mapData, false);
            var path = validator.FindPath();

            return path != null ? 100 : 0;
        }

        // 2. Adapter chuyển đổi mảng thành JSON MapData
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
                    TileType baseTile = baseMap[x, y];

                    if (baseTile == TileType.Wall) cell.objects.Add(new MapValidatorTester.ObjectData { type = "wall" });
                    if (baseTile == TileType.Table) cell.objects.Add(new MapValidatorTester.ObjectData { type = "table" });
                    if (baseTile == TileType.Lamp) cell.objects.Add(new MapValidatorTester.ObjectData { type = "lamp" });
                    if (baseTile == TileType.ExitDoor) cell.objects.Add(new MapValidatorTester.ObjectData { type = "exit" });
                    if (baseTile == TileType.Miasma) cell.objects.Add(new MapValidatorTester.ObjectData { type = "hazard" });

                    // Safe Zone 1x3
                    if (x == spawnPos.x && Mathf.Abs(y - spawnPos.y) <= 1)
                        cell.objects.Add(new MapValidatorTester.ObjectData { type = "safe_path" });

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