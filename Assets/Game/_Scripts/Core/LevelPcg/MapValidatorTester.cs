using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Game.Data;

namespace Game.Core.LevelPCG
{
    /// <summary>
    /// Inspector-based Map Validator Tester
    /// Drag & drop JSON file và bấm "Validate Map" button hoặc context menu
    /// </summary>
    public class MapValidatorTester : MonoBehaviour
    {
        [SerializeField] private TextAsset mapJsonFile;
        [SerializeField] private List<TextAsset> mapsToTest = new();
        [SerializeField] private bool debugMode = false;

        // ==================== DATA MODELS ====================
        [System.Serializable]
        public class MapData
        {
            public int width;
            public int height;
            public Position spawnPos;
            public Position exitPos;
            public Position keyPos;
            public List<Cell> cells = new();
        }

        [System.Serializable]
        public class Position
        {
            public int x;
            public int y;
        }

        [System.Serializable]
        public class Cell
        {
            public int x;
            public int y;
            public List<ObjectData> objects = new();
        }

        [System.Serializable]
        public class ObjectData
        {
            public string type;
            public int layer;
        }

        [System.Serializable]
        public class ValidationResult
        {
            public string mapName;
            public bool isValid;
            public List<string> path = new();
            public string failureReason;
            public int statesExplored;
            public float executionTimeMs;
        }

        // ==================== PUBLIC METHODS ====================

        [ContextMenu("🎮 Validate Single Map")]
        public void ValidateMapFromInspector()
        {
            if (mapJsonFile == null)
            {
                Debug.LogError("[MapValidatorTester] ❌ Please assign a JSON file in Inspector!");
                return;
            }

            try
            {
                string mapName = mapJsonFile.name;
                var result = ValidateMap(mapJsonFile.text, mapName);
                PrintResult(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MapValidatorTester] Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [ContextMenu("🎮 Validate Batch (Multiple Maps)")]
        public void ValidateBatchMaps()
        {
            if (mapsToTest == null || mapsToTest.Count == 0)
            {
                Debug.LogWarning("[MapValidatorTester] ⚠️ No maps in batch list!");
                return;
            }

            Debug.Log($"<color=cyan>[BATCH TEST] Testing {mapsToTest.Count} maps...</color>");
            int validCount = 0;
            int invalidCount = 0;

            foreach (var mapFile in mapsToTest)
            {
                if (mapFile == null) continue;

                try
                {
                    string mapName = mapFile.name;
                    var result = ValidateMap(mapFile.text, mapName);
                    PrintResult(result);

                    if (result.isValid) validCount++;
                    else invalidCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MapValidatorTester] Error testing {mapFile.name}: {ex.Message}");
                    invalidCount++;
                }
            }

            Debug.Log($"<color=yellow>[BATCH RESULT] Valid: {validCount} ✓ | Invalid: {invalidCount} ✗</color>");
        }

        /// <summary>
        /// Main validation method - returns detailed result
        /// </summary>
        public ValidationResult ValidateMap(string jsonContent, string mapName = "Unknown")
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = new ValidationResult { mapName = mapName };

            try
            {
                // Parse JSON (Unity built-in JSON is limited, so manual parse)
                MapData mapData = ParseMapJson(jsonContent);

                // Run validator
                var validator = new MapValidator(mapData, debugMode);
                var path = validator.FindPath();

                stopwatch.Stop();
                result.executionTimeMs = stopwatch.ElapsedMilliseconds;
                result.statesExplored = validator.StatesExplored;

                if (path != null)
                {
                    result.isValid = true;
                    result.path = path;
                }
                else
                {
                    result.isValid = false;
                    result.failureReason = "BFS could not find valid path to exit";
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.isValid = false;
                result.failureReason = ex.Message;
                result.executionTimeMs = stopwatch.ElapsedMilliseconds;
                throw;
            }
        }

        // ==================== PARSING ====================

        private MapData ParseMapJson(string json)
        {
            var data = new MapData();

            // Simple JSON parsing (since Unity's built-in is limited)
            // Extract basic fields
            if (!TryExtractInt(json, "\"width\"", out int width))
                throw new Exception("Cannot find 'width' in JSON");
            if (!TryExtractInt(json, "\"height\"", out int height))
                throw new Exception("Cannot find 'height' in JSON");

            data.width = width;
            data.height = height;

            // Extract spawn position
            data.spawnPos = ExtractPosition(json, "\"spawnPos\"");
            data.exitPos = ExtractPosition(json, "\"exitPos\"");
            data.keyPos = ExtractPosition(json, "\"keyPos\"");

            // Extract cells array
            int cellsStart = json.IndexOf("\"cells\"");
            if (cellsStart == -1)
                throw new Exception("Cannot find 'cells' array in JSON");

            int arrayStart = json.IndexOf("[", cellsStart);
            int arrayEnd = json.LastIndexOf("]");

            if (arrayStart == -1 || arrayEnd == -1)
                throw new Exception("Invalid cells array");

            string cellsJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

            // Parse individual cells (crude but works)
            int depth = 0;
            string currentCell = "";

            for (int i = 0; i < cellsJson.Length; i++)
            {
                char c = cellsJson[i];

                if (c == '{') depth++;
                if (c == '}') depth--;

                currentCell += c;

                if (depth == 0 && c == '}')
                {
                    try
                    {
                        var cell = ParseCell(currentCell);
                        data.cells.Add(cell);
                    }
                    catch
                    {
                        // Skip malformed cells
                    }
                    currentCell = "";
                }
            }

            return data;
        }

        private Position ExtractPosition(string json, string key)
        {
            int start = json.IndexOf(key);
            if (start == -1) return new Position { x = 0, y = 0 };

            start = json.IndexOf("{", start);
            int end = json.IndexOf("}", start);

            string posJson = json.Substring(start, end - start + 1);

            var pos = new Position();
            TryExtractInt(posJson, "\"x\"", out pos.x);
            TryExtractInt(posJson, "\"y\"", out pos.y);

            return pos;
        }

        private Cell ParseCell(string cellJson)
        {
            var cell = new Cell();

            TryExtractInt(cellJson, "\"x\"", out cell.x);
            TryExtractInt(cellJson, "\"y\"", out cell.y);

            // Parse objects array
            int objStart = cellJson.IndexOf("\"objects\"");
            if (objStart != -1)
            {
                int arrayStart = cellJson.IndexOf("[", objStart);
                int arrayEnd = cellJson.IndexOf("]", arrayStart);

                if (arrayStart != -1 && arrayEnd != -1)
                {
                    string objsJson = cellJson.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

                    // Parse each object
                    int depth = 0;
                    string currentObj = "";

                    for (int i = 0; i < objsJson.Length; i++)
                    {
                        char c = objsJson[i];

                        if (c == '{') depth++;
                        if (c == '}') depth--;

                        currentObj += c;

                        if (depth == 0 && c == '}')
                        {
                            var obj = ParseObject(currentObj);
                            if (obj != null)
                                cell.objects.Add(obj);
                            currentObj = "";
                        }
                    }
                }
            }

            return cell;
        }

        private ObjectData ParseObject(string objJson)
        {
            var obj = new ObjectData();

            // Extract type
            int typeStart = objJson.IndexOf("\"type\"");
            if (typeStart != -1)
            {
                int colonPos = objJson.IndexOf(":", typeStart);
                int quoteStart = objJson.IndexOf("\"", colonPos);
                int quoteEnd = objJson.IndexOf("\"", quoteStart + 1);

                if (quoteStart != -1 && quoteEnd != -1)
                {
                    obj.type = objJson.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                }
            }

            TryExtractInt(objJson, "\"layer\"", out obj.layer);

            return obj;
        }

        private bool TryExtractInt(string json, string key, out int value)
        {
            value = 0;

            int start = json.IndexOf(key);
            if (start == -1) return false;

            int colonPos = json.IndexOf(":", start);
            if (colonPos == -1) return false;

            int numStart = colonPos + 1;
            while (numStart < json.Length && (json[numStart] == ' ' || json[numStart] == ',')) numStart++;

            int numEnd = numStart;
            while (numEnd < json.Length && char.IsDigit(json[numEnd])) numEnd++;

            if (numStart >= numEnd) return false;

            return int.TryParse(json.Substring(numStart, numEnd - numStart), out value);
        }

        // ==================== RESULT PRINTING ====================

        private void PrintResult(ValidationResult result)
        {
            if (result.isValid)
            {
                Debug.Log($"<color=green>✅ [{result.mapName}] VALID - Path found!</color>");
                Debug.Log($"<color=cyan>📍 Path ({result.path.Count} steps):</color>");

                for (int i = 0; i < result.path.Count; i++)
                {
                    Debug.Log($"  {i + 1}. {result.path[i]}");
                }

                Debug.Log($"<color=yellow>📊 {result.statesExplored} states | {result.executionTimeMs}ms</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>❌ [{result.mapName}] INVALID - {result.failureReason}</color>");
                Debug.Log($"<color=yellow>⏱️ {result.executionTimeMs}ms</color>");
            }
        }

        // 🚀 NESTED CLASS: MapValidator as Single Source of Truth
        public class MapValidator
        {
            // 💡 GIẢI PHÁP CỦA BẠN: MULTI-LAYER CELL
            public class CellData
            {
                public bool IsWall;
                public bool HasTable;
                public bool HasLamp;
                public bool HasFlower;
                public bool HasKey;
                public bool HasLighter;
                public bool IsExit;
                public bool IsDangerous; // Vực thẳm thực sự
            }

            public class MapState
            {
                public int X, Y;
                public bool HasLighter, HasKey;
                public int FlowerCount, PickedFlowersMask, ActiveLampsMask;
                public int FlowerDirX, FlowerDirY, FlowerStepsLeft;
                public int StepCount;
                public MapState Parent;

                public string GetStateHash() => $"{X},{Y}_{HasLighter}_{HasKey}_{FlowerCount}_{PickedFlowersMask}_{ActiveLampsMask}_{FlowerDirX},{FlowerDirY}_{FlowerStepsLeft}";
                public MapState Clone()
                {
                    return new MapState
                    {
                        X = this.X,
                        Y = this.Y,
                        HasLighter = this.HasLighter,
                        HasKey = this.HasKey,
                        FlowerCount = this.FlowerCount,
                        PickedFlowersMask = this.PickedFlowersMask,
                        ActiveLampsMask = this.ActiveLampsMask,
                        FlowerDirX = this.FlowerDirX,
                        FlowerDirY = this.FlowerDirY,
                        FlowerStepsLeft = this.FlowerStepsLeft,
                        StepCount = this.StepCount,
                        Parent = this.Parent
                    };
                }
            }

            private CellData[,] map;
            private List<(int x, int y)> lamps = new();
            private List<(int x, int y)> flowers = new();
            private (int x, int y) spawn, exit;
            private int width, height;

            private readonly int[] dx = { 0, 1, 0, -1 };
            private readonly int[] dy = { -1, 0, 1, 0 };
            private const int MAX_FLOWERS = 5;
            public int StatesExplored { get; private set; } = 0;

            public MapValidator(MapValidatorTester.MapData data, bool debugMode = false)
            {
                width = data.width;
                height = data.height;
                map = new CellData[width, height];

                spawn = (data.spawnPos.x, data.spawnPos.y);
                exit = (data.exitPos.x, data.exitPos.y);

                // Khởi tạo toàn bộ CellData
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        map[x, y] = new CellData();

                foreach (var cell in data.cells)
                {
                    bool hasHazard = false;
                    bool hasSafePath = false;

                    foreach (var obj in cell.objects)
                    {
                        if (obj.type == "wall") map[cell.x, cell.y].IsWall = true;
                        if (obj.type == "table") map[cell.x, cell.y].HasTable = true;
                        if (obj.type == "lamp") { map[cell.x, cell.y].HasLamp = true; lamps.Add((cell.x, cell.y)); }
                        if (obj.type == "flower") { map[cell.x, cell.y].HasFlower = true; flowers.Add((cell.x, cell.y)); }
                        if (obj.type == "key") map[cell.x, cell.y].HasKey = true;
                        if (obj.type == "lighter") map[cell.x, cell.y].HasLighter = true;
                        if (obj.type == "exit") map[cell.x, cell.y].IsExit = true;
                        if (obj.type == "hazard") hasHazard = true;
                        if (obj.type == "safe_path") hasSafePath = true;
                    }

                    bool isInSafeZone = cell.x == spawn.x && Mathf.Abs(cell.y - spawn.y) <= 1;
                    map[cell.x, cell.y].IsDangerous = hasHazard && !hasSafePath && !isInSafeZone;
                }
            }

            public List<string> FindPath()
            {
                var visited = new HashSet<string>();
                var queue = new Queue<MapState>();

                MapState startState = new MapState { X = spawn.x, Y = spawn.y, Parent = null };
                queue.Enqueue(startState);
                visited.Add(startState.GetStateHash());

                int iterations = 0;
                const int maxIterations = 200000;

                int[] dxInteract = { 0, 0, -1, 1, 1, 1, -1, -1 };
                int[] dyInteract = { -1, 1, 0, 0, 1, -1, 1, -1 };

                while (queue.Count > 0 && iterations < maxIterations)
                {
                    iterations++;
                    MapState rawState = queue.Dequeue();
                    StatesExplored++;

                    if (rawState.X == exit.x && rawState.Y == exit.y && rawState.HasKey)
                    {
                        var path = new List<string>();
                        for (var curr = rawState; curr != null; curr = curr.Parent) path.Add($"({curr.X},{curr.Y})");
                        path.Reverse();
                        return path;
                    }

                    // 🚨 BÍ QUYẾT: Tạo ra 1 State riêng để nhặt đồ, giữ nguyên rawState cho lịch sử
                    MapState stateAfterPickup = rawState.Clone();

                    // 🧲 PHASE 1A: NHẶT ĐỒ
                    for (int d = 0; d < 4; d++)
                    {
                        int ax = stateAfterPickup.X + dx[d];
                        int ay = stateAfterPickup.Y + dy[d];
                        if (ax < 0 || ax >= width || ay < 0 || ay >= height) continue;
                        var adjCell = map[ax, ay];

                        if (adjCell.HasLighter) stateAfterPickup.HasLighter = true;
                        if (adjCell.HasKey) stateAfterPickup.HasKey = true;
                        if (adjCell.HasFlower)
                        {
                            int flowerID = flowers.IndexOf((ax, ay));
                            if (flowerID >= 0 && (stateAfterPickup.PickedFlowersMask & (1 << flowerID)) == 0)
                            {
                                if (stateAfterPickup.FlowerCount < MAX_FLOWERS)
                                {
                                    stateAfterPickup.FlowerCount++;
                                    stateAfterPickup.PickedFlowersMask |= (1 << flowerID);
                                }
                            }
                        }
                    }

                    // 🧲 PHASE 1B: BẬT ĐÈN
                    if (stateAfterPickup.HasLighter)
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            int ax = stateAfterPickup.X + dx[d];
                            int ay = stateAfterPickup.Y + dy[d];
                            if (ax < 0 || ax >= width || ay < 0 || ay >= height) continue;
                            if (map[ax, ay].HasLamp)
                            {
                                int lampID = lamps.IndexOf((ax, ay));
                                if (lampID >= 0) stateAfterPickup.ActiveLampsMask |= (1 << lampID);
                            }
                        }
                    }

                    // 🧲 PHASE 2: DI CHUYỂN BẰNG CELLDATA
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = stateAfterPickup.X + dx[d];
                        int ny = stateAfterPickup.Y + dy[d];

                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                        var cell = map[nx, ny];

                        // 🚨 ĐIỀU KIỆN CHẶN ĐƯỜNG MỚI: Rõ ràng, sạch sẽ, không ảo giác!
                        if (cell.IsWall || cell.HasTable) continue;

                        MapState nextState = new MapState
                        {
                            X = nx,
                            Y = ny,
                            HasLighter = stateAfterPickup.HasLighter,
                            HasKey = stateAfterPickup.HasKey,
                            FlowerCount = stateAfterPickup.FlowerCount,
                            PickedFlowersMask = stateAfterPickup.PickedFlowersMask,
                            ActiveLampsMask = stateAfterPickup.ActiveLampsMask,
                            FlowerDirX = stateAfterPickup.FlowerDirX,
                            FlowerDirY = stateAfterPickup.FlowerDirY,
                            FlowerStepsLeft = stateAfterPickup.FlowerStepsLeft,
                            StepCount = stateAfterPickup.StepCount + 1,
                            Parent = stateAfterPickup
                        };

                        bool isNaturallyLit = false;
                        if (nx == spawn.x && Mathf.Abs(ny - spawn.y) <= 1) isNaturallyLit = true;

                        for (int i = 0; i < lamps.Count; i++)
                        {
                            if ((nextState.ActiveLampsMask & (1 << i)) != 0 && Mathf.Abs(nx - lamps[i].x) <= 1 && Mathf.Abs(ny - lamps[i].y) <= 1)
                            {
                                isNaturallyLit = true; break;
                            }
                        }

                        if (isNaturallyLit)
                        {
                            nextState.FlowerStepsLeft = 0; nextState.FlowerDirX = 0; nextState.FlowerDirY = 0;
                        }
                        else
                        {
                            bool ridingBeam = false;
                            if (stateAfterPickup.FlowerStepsLeft > 0 && stateAfterPickup.FlowerDirX == dx[d] && stateAfterPickup.FlowerDirY == dy[d])
                            {
                                ridingBeam = true;
                                nextState.FlowerStepsLeft = stateAfterPickup.FlowerStepsLeft - 1;
                            }

                            if (!ridingBeam)
                            {
                                if (nextState.FlowerCount > 0)
                                {
                                    nextState.FlowerCount--;
                                    nextState.FlowerDirX = dx[d]; nextState.FlowerDirY = dy[d];
                                    nextState.FlowerStepsLeft = 2;
                                }
                                else continue;
                            }
                        }

                        string stateKey = nextState.GetStateHash();
                        if (!visited.Contains(stateKey))
                        {
                            visited.Add(stateKey);
                            queue.Enqueue(nextState);
                        }
                    }
                }
                return null;
            }

            private List<string> ReconstructPath(MapState goalState)
            {
                var pathSteps = new List<string>();
                var path = new List<MapState>();
                MapState current = goalState;

                while (current != null)
                {
                    path.Add(current);
                    current = current.Parent;
                }
                path.Reverse();

                for (int i = 0; i < path.Count; i++)
                {
                    var state = path[i];
                    string info = $"({state.X},{state.Y}) | Hoa: {state.FlowerCount} | Tia sáng còn: {state.FlowerStepsLeft}";

                    if (i > 0)
                    {
                        var prev = path[i - 1];
                        if (state.HasLighter && !prev.HasLighter) info += " <color=orange>[🔥 Nhặt Bật lửa]</color>";
                        if (state.HasKey && !prev.HasKey) info += " <color=yellow>[🔑 Nhặt Chìa khóa]</color>";

                        // Tính số hoa nhặt được bằng cách so sánh bitmask
                        int newFlowers = CountBits(state.PickedFlowersMask) - CountBits(prev.PickedFlowersMask);
                        for (int f = 0; f < newFlowers; f++) info += " <color=magenta>[🌸 Nhặt Hoa]</color>";

                        if (state.ActiveLampsMask != prev.ActiveLampsMask) info += " <color=cyan>[💡 Bật Đèn]</color>";
                    }
                    else
                    {
                        info += " <color=green>[🏁 SPAWN]</color>";
                    }

                    pathSteps.Add(info);
                    Debug.Log(info);
                }
                return pathSteps;
            }

            // Hàm hỗ trợ đếm số bit 1 để biết đã nhặt bao nhiêu hoa
            private int CountBits(int n)
            {
                int count = 0;
                while (n > 0) { count += n & 1; n >>= 1; }
                return count;
            }
        }  // End MapValidator class
    }  // End MapValidatorTester class
}
