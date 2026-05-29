using Game.Core.LevelPCG;
using Game.Core.Services;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    public class LevelLoader : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private Transform environmentParent;

        // 🧲 ENVIRONMENT MANAGEMENT - Quản lý các object environment được sinh ra
        private List<GameObject> environmentObjects = new List<GameObject>();
        private Dictionary<string, Transform> environmentFolders = new Dictionary<string, Transform>(); // Dictionary không serialize được

        [Header("Tilemap References")]


        [Header("Position Settings")]
        [SerializeField] private Vector2 mapAnchor = new Vector2(0f, 0f);
        [SerializeField] private string testLevelName = "Map_00001"; // Dùng cho TestReloadLevel
        [SerializeField] private float tileSpacing = 1f;

        [Tooltip("Dùng biến này ngoài Inspector để xê dịch toàn bộ lõi map lọt vào trong tường (Gợi ý: X=1, Y=1)")]
        [SerializeField] private Vector2 tileOffset = Vector2.zero;

        [Header("Prefab References")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject miasmaHazardPrefab;
        [SerializeField] private GameObject tablePrefab;
        [SerializeField] private GameObject lampPrefab;
        [SerializeField] private GameObject lighterPrefab;
        [SerializeField] private GameObject keyPrefab;
        [SerializeField] private GameObject exitDoorPrefab;
        [SerializeField] private GameObject flowerPrefab;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject safePathPrefab;

        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Coroutine loadingCoroutine;
        private int mapMaxY = 0; // Lưu trữ chiều cao thực tế của map để lật trục Y

        private const string LEVELS_RESOURCE_PATH = "Levels/";
        private const string JSON_EXTENSION = ".json";

        private void Awake()
        {
            if (gridParent == null)
            {
                GameObject gridRoot = new GameObject("GridParent");
                gridRoot.transform.SetParent(transform);
                gridParent = gridRoot.transform;
            }
            // 🧲 Đảm bảo gốc Grid là (0,0,0)
            gridParent.localPosition = Vector3.zero;

            // 🧲 GỘP ENVIRONMENT VÀO LÀM CON CỦA GRID
            if (environmentParent == null)
            {
                GameObject envObj = new GameObject("Environment");
                envObj.transform.SetParent(gridParent);
                envObj.transform.localPosition = Vector3.zero;
                environmentParent = envObj.transform;
            }

            ServiceLocator.Instance.Register<ILevelService>(new LevelService(this));
        }

        /// <summary>
        /// Test reload level - Dùng để test load level mà không cần chạy scene
        /// </summary>
        [ContextMenu("Test: Reload Level (Clear + Load)")]
        public void TestReloadLevel()
        {
            if (string.IsNullOrEmpty(testLevelName))
            {
                Debug.LogError("[LevelLoader] testLevelName is empty!");
                return;
            }

            Debug.Log($"<color=cyan>[TEST] Reloading level: {testLevelName}</color>");
            LoadLevel(testLevelName);
        }

        /// <summary>
        /// Xóa tất cả object đã load (Reset state)
        /// </summary>
        [ContextMenu("Test: Reset Level State (Clear All)")]
        public void TestResetLevelState()
        {
            Debug.Log($"<color=yellow>[TEST] Clearing {spawnedObjects.Count} spawned objects</color>");
            ClearCurrentLevel();
            Debug.Log($"<color=green>[TEST] Level state reset - Ready to reload</color>");
        }

        public void LoadLevel(string levelName)
        {
            ClearCurrentLevel();
            Debug.Log($"[LevelLoader] Loading level: {levelName}");

            LevelData levelData = LoadLevelData(levelName);
            if (levelData == null)
            {
                Debug.LogError($"[LevelLoader] Failed to load level data for: {levelName}");
                return;
            }

            // DÙNG CHIỀU CAO TỬ JSON, KHÔNG QUÉT TILE (Tránh lỗi khi có hàng trống)
            mapMaxY = (levelData.mapHeight > 0) ? levelData.mapHeight - 1 : 9;

            InstantiateLevel(levelData);
            EnablePhysicsAfterLoad();
            PlacePlayer(levelData);
            SetupCamera();

            Debug.Log($"[LevelLoader] Level '{levelName}' loaded successfully! Spawned {spawnedObjects.Count} objects");
        }

        public void LoadLevelAsync(string levelName, System.Action onComplete = null)
        {
            if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);
            loadingCoroutine = StartCoroutine(LoadLevelAsyncRoutine(levelName, onComplete));
        }

        private IEnumerator LoadLevelAsyncRoutine(string levelName, System.Action onComplete)
        {
            Debug.Log($"[LevelLoader] Starting async level load: {levelName}");

            LevelData levelData = LoadLevelData(levelName);
            if (levelData == null)
            {
                Debug.LogError($"[LevelLoader] Failed to load level data (async) for: {levelName}");
                yield break;
            }

            // DÙNG CHIỀU CAO TỬ JSON, KHÔNG QUÉT TILE (Tránh lỗi khi có hàng trống)
            mapMaxY = (levelData.mapHeight > 0) ? levelData.mapHeight - 1 : 9;

            Debug.Log($"[LevelLoader] Async loading level data - Total tiles: {levelData.tiles.Count}");

            yield return StartCoroutine(InstantiateLevelAsync(levelData));

            EnablePhysicsAfterLoad();
            PlacePlayer(levelData);
            SetupCamera();

            Debug.Log($"[LevelLoader] Async level '{levelName}' loaded successfully! Spawned {spawnedObjects.Count} objects");

            onComplete?.Invoke();
            loadingCoroutine = null;
        }
        private Vector3 GetRawGridPosition(int x, int y)
        {
            // 🧲 LỚP KEO DÁN: CHỈ TÍNH TỌA ĐỘ ĐINH GHİM THÔ (RAW POSITION)
            // Mọi điều chỉnh pivot sẽ được xử lý tự động bởi bounds.center trong InstantiateTile

            // 1. Lật trục Y chuẩn xác
            float invertedY = mapMaxY - y;

            // 2. Tính toán tọa độ đinh ghim thô theo lưới
            float rawX = (x * tileSpacing) + mapAnchor.x + tileOffset.x;
            float rawY = (invertedY * tileSpacing) + mapAnchor.y + tileOffset.y;

            return new Vector3(rawX, rawY, 0);
        }
        private LevelData LoadLevelData(string levelName)
        {
            try
            {
                TextAsset levelAsset = Resources.Load<TextAsset>(LEVELS_RESOURCE_PATH + levelName);
                string jsonText = null;

                if (levelAsset == null)
                {
                    string filePath = Path.Combine(Application.dataPath, "Game/Resources", LEVELS_RESOURCE_PATH, levelName + JSON_EXTENSION);
                    if (!File.Exists(filePath)) return null;
                    jsonText = File.ReadAllText(filePath);
                }
                else
                {
                    jsonText = levelAsset.text;
                }

                if (jsonText.Contains("\"cells\":")) return LoadCellBasedLevelData(jsonText);
                return JsonUtility.FromJson<LevelData>(jsonText);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LevelLoader] Error: {ex.Message}");
                return null;
            }
        }

        private LevelData LoadCellBasedLevelData(string jsonText)
        {
            CellBasedLevelData cellData = JsonUtility.FromJson<CellBasedLevelData>(jsonText);
            if (cellData == null || cellData.cells == null) return null;

            LevelData levelData = new LevelData
            {
                levelID = cellData.levelID,
                seed = cellData.seed,
                generationCount = cellData.generationCount,
                maxLight = cellData.maxLight,
                spawnPos = cellData.spawnPos,
                mapHeight = cellData.height,  // LƯU CHIỀU CAO TỬ JSON ĐỂ TRÁNH QUÉT TILE
                tiles = new List<TileData>(),
                safePaths = new List<TileData>()
            };

            foreach (CellData cell in cellData.cells)
            {
                if (cell.objects == null || cell.objects.Count == 0) continue;

                var sortedObjects = new List<CellObjectData>(cell.objects);
                sortedObjects.Sort((a, b) => a.layer.CompareTo(b.layer));

                foreach (var cellObject in sortedObjects)
                {
                    string tileType = ConvertCellObjectToTileType(cellObject);
                    if (!string.IsNullOrEmpty(tileType) && tileType != "Empty")
                    {
                        levelData.tiles.Add(new TileData { type = tileType, x = cell.x, y = cell.y, layer = cellObject.layer });
                    }
                }
            }
            return levelData;
        }



        private string ConvertCellObjectToTileType(CellObjectData cellObject)
        {
            string typeNormalized = cellObject.type?.ToLowerInvariant() ?? "empty";
            return typeNormalized switch
            {
                "wall" => nameof(TileType.Wall),
                "table" => nameof(TileType.Table),
                "key" => nameof(TileType.Key),
                "lighter" => nameof(TileType.Lighter),
                "lamp" => nameof(TileType.Lamp),
                "flower" => nameof(TileType.Flower),
                "exit" => nameof(TileType.ExitDoor),
                "hazard" => nameof(TileType.Miasma),
                "ground" => "Empty",
                _ => "Empty"
            };
        }

        private void InstantiateLevel(LevelData levelData)
        {
            Debug.Log($"[LevelLoader] Starting level instantiation - Total tiles: {levelData.tiles.Count}");

            // BỨC TƯỜNG BỊ FIX CỨNG TẠI ĐIỂM NEO (KHÔNG BỊ ẢNH HƯỞNG BỞI OFFSET)
            if (wallPrefab != null)
            {
                Vector3 wallPosition = new Vector3(mapAnchor.x, mapAnchor.y, 1f);
                GameObject wallInstance = Instantiate(wallPrefab, wallPosition, Quaternion.identity);
                wallInstance.name = "Wall_Background_Grid";
                // 🧲 Wall là background cố định - KHÔNG áp dụng adhesive layer
                OrganizeTileInHierarchy(wallInstance, nameof(TileType.Wall));
                spawnedObjects.Add(wallInstance);
            }

            var sortedTiles = new List<TileData>(levelData.tiles);
            sortedTiles.Sort((a, b) => a.layer.CompareTo(b.layer));

            foreach (TileData tile in sortedTiles)
            {
                if (tile.type != nameof(TileType.Wall)) InstantiateTile(tile);
            }

            if (levelData.safePaths == null || levelData.safePaths.Count == 0) GenerateSafePathsAroundSpawn(levelData);
            if (levelData.safePaths != null && levelData.safePaths.Count > 0)
            {
                foreach (TileData safePath in levelData.safePaths) InstantiateTile(safePath);
            }
        }

        private IEnumerator InstantiateLevelAsync(LevelData levelData)
        {
            ClearCurrentLevel();

            if (wallPrefab != null)
            {
                Vector3 wallPosition = new Vector3(mapAnchor.x, mapAnchor.y, -1f);
                GameObject wallInstance = Instantiate(wallPrefab, wallPosition, Quaternion.identity);
                wallInstance.name = "Wall_Background_Grid";
                OrganizeTileInHierarchy(wallInstance, nameof(TileType.Wall));
                spawnedObjects.Add(wallInstance);
                yield return null;
            }

            var sortedTiles = new List<TileData>(levelData.tiles);
            sortedTiles.Sort((a, b) => a.layer.CompareTo(b.layer));

            int batchSize = 500;
            int count = 0;
            int totalProcessed = 0;

            foreach (TileData tile in sortedTiles)
            {
                if (tile.type != nameof(TileType.Wall))
                {
                    InstantiateTile(tile);
                    count++;
                    totalProcessed++;
                    if (count >= batchSize)
                    {
                        Debug.Log($"[LevelLoader] Async batch progress: {totalProcessed}/{sortedTiles.Count} tiles loaded");
                        yield return null;
                        count = 0;
                    }
                }
            }

            if (levelData.safePaths == null || levelData.safePaths.Count == 0) GenerateSafePathsAroundSpawn(levelData);
            if (levelData.safePaths != null && levelData.safePaths.Count > 0)
            {
                foreach (TileData safePath in levelData.safePaths) InstantiateTile(safePath);
                yield return null;
            }
        }

        private void InstantiateTile(TileData tile)
        {
            GameObject prefab = GetPrefabForTile(tile.type);
            if (prefab == null) return;

            // 1. LẤY TỌA ĐỘ ĐINH GHİM THÔ
            Vector3 rawPosition = GetRawGridPosition(tile.x, tile.y);

            // 2. SINH VẬT THỂ TẠM THỜI TẠI VỊ TRÍ THÔ
            GameObject instance = Instantiate(prefab, rawPosition, Quaternion.identity);
            instance.name = $"{tile.type}_{tile.x}_{tile.y}_L{tile.layer}";

            // ==========================================
            // 🧲 LỚP KEO DÁN (FIXED): Ép thẳng Transform vào tâm ô lưới
            // ==========================================
            SpriteRenderer sr = instance.GetComponentInChildren<SpriteRenderer>();

            // Tính tâm lý tưởng của ô lưới
            Vector3 cellCenter = new Vector3(
                rawPosition.x + (tileSpacing / 2f),
                rawPosition.y + (tileSpacing / 2f),
                0
            );

            // BỎ bù trừ bounds. Ép thẳng tọa độ vào tâm!
            instance.transform.position = cellCenter;

            if (sr != null)
            {
                sr.sortingOrder = tile.layer;
            }
            // ==========================================

            // Xử lý ngoại lệ: Cửa thoát nằm ở mép phải của ô lưới
            if (tile.type == nameof(TileType.ExitDoor))
            {
                instance.transform.position += new Vector3(1f, 0, 0);
            }

            // 🧲 LỚP KEO DÁN: Reset scale cho tất cả objects
            bool isPrefabItem = (tile.type == nameof(TileType.Flower) || tile.type == nameof(TileType.Lamp) ||
                               tile.type == nameof(TileType.Lighter) || tile.type == nameof(TileType.Key) ||
                               tile.type == nameof(TileType.SafePath) || tile.type == nameof(TileType.ExitDoor) ||
                               tile.type == nameof(TileType.Table) || tile.type == nameof(TileType.Miasma));
            if (isPrefabItem)
            {
                instance.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            // Khóa vật lý
            Rigidbody2D[] allRigidbodies = instance.GetComponentsInChildren<Rigidbody2D>();
            foreach (Rigidbody2D rbChild in allRigidbodies)
            {
                if (rbChild != null)
                {
                    rbChild.bodyType = RigidbodyType2D.Static;
                    rbChild.linearVelocity = Vector2.zero;
                    rbChild.angularVelocity = 0;
                }
            }

            Collider2D[] allColliders = instance.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in allColliders)
            {
                if (col != null && col.enabled) col.enabled = false;
            }

            bool isPrefabObject = (tile.type == nameof(TileType.Flower) || tile.type == nameof(TileType.Lamp) || tile.type == nameof(TileType.Lighter) || tile.type == nameof(TileType.Key) || tile.type == nameof(TileType.SafePath) || tile.type == nameof(TileType.ExitDoor) ||
                                tile.type == nameof(TileType.Table) || tile.type == nameof(TileType.Miasma));
            if (isPrefabObject) OrganizePrefabInHierarchy(instance, tile.type);
            else OrganizeTileInHierarchy(instance, tile.type);

            spawnedObjects.Add(instance);
        }

        private void GenerateSafePathsAroundSpawn(LevelData levelData)
        {
            if (levelData.safePaths == null) levelData.safePaths = new List<TileData>();
            Vector2Int spawnPos = levelData.spawnPos;
            for (int y = spawnPos.y - 1; y <= spawnPos.y + 1; y++)
            {
                if (y < 0 || y >= 20) continue;
                if (!levelData.safePaths.Exists(sp => sp.x == spawnPos.x && sp.y == y))
                {
                    levelData.safePaths.Add(new TileData { type = nameof(TileType.SafePath), x = spawnPos.x, y = y });
                }
            }
        }

        private void PlacePlayer(LevelData levelData)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // 🧲 LỚP KEO DÁN: SỬ DỤNG RAW POSITION + BOUNDS OFFSET
            Vector3 rawSpawnPos = GetRawGridPosition(levelData.spawnPos.x, levelData.spawnPos.y);

            if (player == null && playerPrefab != null)
            {
                player = Instantiate(playerPrefab, rawSpawnPos, Quaternion.identity);
                player.tag = "Player";
                player.transform.localScale = new Vector3(1f, 1f, 1f); // 🧲 Reset scale

                // ==========================================
                // Áp dụng lớp keo dán cho player (FIXED)
                // ==========================================
                Vector3 cellCenter = new Vector3(
                    rawSpawnPos.x + (tileSpacing / 2f),
                    rawSpawnPos.y + (tileSpacing / 2f),
                    0
                );

                // Ép tọa độ Y: lấy phần nguyên, dương thì thêm .7, âm thì trừ .3
                float yInt = (int)cellCenter.y;
                float fixedY = cellCenter.y >= 0 ? yInt + 0.7f : yInt - 0.3f;

                // Ép Player vào đúng tâm X, và Y đã được chỉnh sửa
                player.transform.position = new Vector3(cellCenter.x, fixedY, cellCenter.z);

                SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    // Giữ nguyên logic xử lý layer nếu có
                }
                // ==========================================

                if (player.GetComponent<Game.Gameplay.Player.PlayerInput>() == null)
                    player.AddComponent<Game.Gameplay.Player.PlayerInput>();
            }
            else if (player != null)
            {
                player.transform.localScale = new Vector3(1f, 1f, 1f); // 🧲 Reset scale

                // ==========================================
                // Áp dụng lớp keo dán cho player khi di chuyển (FIXED)
                // ==========================================
                Vector3 cellCenter = new Vector3(
                    rawSpawnPos.x + (tileSpacing / 2f),
                    rawSpawnPos.y + (tileSpacing / 2f),
                    0
                );

                // Ép tọa độ Y: lấy phần nguyên, dương thì thêm .7, âm thì trừ .3
                float yInt = (int)cellCenter.y;
                float fixedY = cellCenter.y >= 0 ? yInt + 0.7f : yInt - 0.3f;

                // Ép Player vào đúng tâm X, và Y đã được chỉnh sửa
                player.transform.position = new Vector3(cellCenter.x, fixedY, cellCenter.z);
                // ==========================================

                var playerController = player.GetComponent<Game.Gameplay.Player.Components.PlayerControllerManager>();
                if (playerController != null) playerController.SetRespawnPoint(player.transform.position);
            }
        }
        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            // 🧲 Camera position tại (8, 4, -10)
            Vector3 cameraPos = new Vector3(8f, 4f, -10f);
            mainCamera.transform.position = cameraPos;
            mainCamera.transform.rotation = Quaternion.identity;
            mainCamera.orthographicSize = 5f; // Độ zoom để nhìn được map
        }

        private void EnablePhysicsAfterLoad()
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj == null) continue;
                Rigidbody2D[] allRbs = obj.GetComponentsInChildren<Rigidbody2D>();
                foreach (Rigidbody2D rb in allRbs)
                {
                    if (rb != null && rb.bodyType == RigidbodyType2D.Static)
                    {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        rb.constraints = RigidbodyConstraints2D.None;
                    }
                }
                Collider2D[] allColliders = obj.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D col in allColliders)
                {
                    if (col != null && !col.enabled) col.enabled = true;
                }
            }
        }

        private GameObject GetPrefabForTile(string tileType)
        {
            string normalized = tileType ?? "Empty";
            return normalized switch
            {
                nameof(TileType.Wall) => wallPrefab,
                nameof(TileType.Miasma) => miasmaHazardPrefab,
                nameof(TileType.Table) => tablePrefab,
                nameof(TileType.Lamp) => lampPrefab,
                nameof(TileType.Lighter) => lighterPrefab,
                nameof(TileType.Key) => keyPrefab,
                nameof(TileType.ExitDoor) => exitDoorPrefab,
                nameof(TileType.Flower) => flowerPrefab,
                nameof(TileType.SafePath) => safePathPrefab,
                _ => null
            };
        }

        private void OrganizeTileInHierarchy(GameObject instance, string tileType)
        {
            if (gridParent == null) return;
            string folderName = tileType switch { nameof(TileType.Miasma) => "Hazards", nameof(TileType.Lighter) => "Items", nameof(TileType.Key) => "Items", nameof(TileType.Flower) => "Items", _ => tileType + "s" };
            Transform folder = gridParent.Find(folderName);
            if (folder == null)
            {
                GameObject folderObj = new GameObject(folderName);
                folderObj.transform.SetParent(gridParent);
                folder = folderObj.transform;
            }
            // 🧲 LỚP KEO DÁN: SetParent(parent, true) = giữ nguyên World Position khi gán parent
            // Nếu dùng true, vật thể sẽ giữ nguyên vị trí thế giới; nếu dùng false, nó sẽ bị tính lại theo toạ độ local
            instance.transform.SetParent(folder, true);
        }

        private void OrganizePrefabInHierarchy(GameObject instance, string tileType)
        {
            if (environmentParent == null) return;
            string folderName = tileType switch { nameof(TileType.Lamp) => "Furniture", nameof(TileType.Lighter) => "Items", nameof(TileType.Key) => "Items", nameof(TileType.Flower) => "Decor", nameof(TileType.SafePath) => "SafeZones", nameof(TileType.ExitDoor) => "Exit", _ => tileType };

            // 🧲 KIỂM TRA FOLDER ĐÃ TRACK TRONG DICTIONARY
            Transform folder;
            if (environmentFolders.ContainsKey(folderName))
            {
                folder = environmentFolders[folderName];
            }
            else
            {
                folder = environmentParent.Find(folderName);
                if (folder == null)
                {
                    GameObject folderObj = new GameObject(folderName);
                    folderObj.transform.SetParent(environmentParent);
                    folder = folderObj.transform;
                    environmentObjects.Add(folderObj); // 🧲 TRACK FOLDER
                }
                environmentFolders[folderName] = folder; // 🧲 LƯU FOLDER VÀO DICTIONARY
            }

            // 🧲 LỚP KEO DÁN: SetParent(parent, true) = giữ nguyên World Position khi gán parent
            instance.transform.SetParent(folder, true);
            environmentObjects.Add(instance); // 🧲 TRACK OBJECT VÀO LIST
        }

        public void ClearCurrentLevel()
        {
            if (loadingCoroutine != null)
            {
                StopCoroutine(loadingCoroutine);
                loadingCoroutine = null;
            }
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null) Destroy(obj);
            }
            spawnedObjects.Clear();

            // 🧲 XÓA ENVIRONMENT OBJECTS
            foreach (GameObject envObj in environmentObjects)
            {
                if (envObj != null) Destroy(envObj);
            }
            environmentObjects.Clear();
            environmentFolders.Clear();
        }

        /// <summary>
        /// Lấy danh sách tất cả các object đã spawn với vị trí của chúng
        /// </summary>
        public List<(GameObject obj, Vector3 position, string name)> GetAllSpawnedObjectsWithPositions()
        {
            var result = new List<(GameObject, Vector3, string)>();
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    result.Add((obj, obj.transform.position, obj.name));
                }
            }
            return result;
        }

        /// <summary>
        /// Lấy vị trí của một object cụ thể theo tên
        /// </summary>
        public Vector3? GetObjectPositionByName(string objectName)
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null && obj.name == objectName)
                {
                    return obj.transform.position;
                }
            }
            return null;
        }

        /// <summary>
        /// Lấy danh sách các object của một loại cụ thể (Type)
        /// </summary>
        public List<(GameObject obj, Vector3 position)> GetObjectsByType(string tileType)
        {
            var result = new List<(GameObject, Vector3)>();
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null && obj.name.Contains(tileType))
                {
                    result.Add((obj, obj.transform.position));
                }
            }
            return result;
        }

        /// <summary>
        /// Lấy tất cả các object trong một khoảng vị trí (Bounds)
        /// </summary>
        public List<(GameObject obj, Vector3 position)> GetObjectsInArea(Vector3 center, float radius)
        {
            var result = new List<(GameObject, Vector3)>();
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    float distance = Vector3.Distance(obj.transform.position, center);
                    if (distance <= radius)
                    {
                        result.Add((obj, obj.transform.position));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Debug: Kiểm tra Transform.position và localPosition của các object tại cell
        /// Dùng: levelLoader.DebugCheckCellPositions(5, 5);
        /// </summary>
        [ContextMenu("Test: Check Cell Positions (Debug Coordinates)")]
        public void DebugCheckCellPositions_Default()
        {
            DebugCheckCellPositions(0, 4);
        }

        /// <summary>
        /// Debug: Kiểm tra Grid Position có phải (0,0,0) không
        /// Nếu Grid không = (0,0,0), toạ độ sẽ bị cộng dồn!
        /// </summary>
        [ContextMenu("Test: Verify Grid Position (Should Be 0,0,0)")]
        public void DebugVerifyGridPosition()
        {
            // 🧲 LỚP KEO DÁN: Đảm bảo gridParent và environmentParent tồn tại
            if (gridParent == null)
            {
                GameObject gridRoot = new GameObject("GridParent");
                gridRoot.transform.SetParent(transform);
                gridParent = gridRoot.transform;
                gridParent.localPosition = Vector3.zero;
            }

            if (environmentParent == null)
            {
                GameObject envObj = new GameObject("Environment");
                envObj.transform.SetParent(transform);
                environmentParent = envObj.transform;
                environmentParent.localPosition = Vector3.zero;
            }

            Debug.Log($"\n{'='.ToString().PadRight(80, '=')}");
            Debug.Log($"[LevelLoader] DEBUG: VERIFY GRID POSITION");
            Debug.Log($"{'='.ToString().PadRight(80, '=')}");

            Vector3 gridPos = gridParent.position;
            Debug.Log($"\n📍 Grid Position (World): ({gridPos.x:F4}, {gridPos.y:F4}, {gridPos.z:F4})");

            if (Mathf.Abs(gridPos.x) < 0.001f && Mathf.Abs(gridPos.y) < 0.001f && Mathf.Abs(gridPos.z) < 0.001f)
            {
                Debug.Log($"✅ Grid Position = (0, 0, 0) - CHÍNH XÁC!");
            }
            else
            {
                Debug.LogWarning($"❌ Grid Position KHÔNG = (0, 0, 0) - TOẠ ĐỘ SẼ BỊ LỆCH!");
                Debug.LogWarning($"   Hãy reset Grid Position về (0, 0, 0) trong Inspector hoặc chạy:");
                Debug.LogWarning($"   gridParent.position = Vector3.zero;");
            }

            Vector3 envPos = environmentParent.position;
            Debug.Log($"\n📍 Environment Position (World): ({envPos.x:F4}, {envPos.y:F4}, {envPos.z:F4})");

            if (Mathf.Abs(envPos.x) < 0.001f && Mathf.Abs(envPos.y) < 0.001f && Mathf.Abs(envPos.z) < 0.001f)
            {
                Debug.Log($"✅ Environment Position = (0, 0, 0) - CHÍNH XÁC!");
            }
            else
            {
                Debug.LogWarning($"❌ Environment Position KHÔNG = (0, 0, 0) - TOẠ ĐỘ SẼ BỊ LỆCH!");
                Debug.LogWarning($"   Hãy reset Environment Position về (0, 0, 0)");
            }

            Debug.Log($"\n{'='.ToString().PadRight(80, '=')}");
        }

        public void DebugCheckCellPositions(int gridX, int gridY)
        {
            Debug.Log($"\n{'='.ToString().PadRight(80, '=')}");
            Debug.Log($"[LevelLoader] DEBUG: KIỂM TRA TỌA ĐỘ TẠI CELL ({gridX}, {gridY})");
            Debug.Log($"{'='.ToString().PadRight(80, '=')}");

            List<(GameObject obj, Vector3 worldPos, Vector3 localPos, string type)> foundObjects = new List<(GameObject, Vector3, Vector3, string)>();

            foreach (GameObject obj in spawnedObjects)
            {
                if (obj == null) continue;

                string name = obj.name;
                string[] parts = name.Split('_');

                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[1], out int objX) && int.TryParse(parts[2], out int objY))
                    {
                        if (objX == gridX && objY == gridY)
                        {
                            string type = parts[0];
                            Vector3 worldPosition = obj.transform.position;
                            Vector3 localPosition = obj.transform.localPosition;

                            foundObjects.Add((obj, worldPosition, localPosition, type));

                            Debug.Log($"\n📍 Object: {name}");
                            Debug.Log($"   Type: {type}");
                            Debug.Log($"   Transform.position (World):      ({worldPosition.x:F4}, {worldPosition.y:F4}, {worldPosition.z:F4})");
                            Debug.Log($"   Transform.localPosition (Local): ({localPosition.x:F4}, {localPosition.y:F4}, {localPosition.z:F4})");

                            var spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                Debug.Log($"   Sorting Order: {spriteRenderer.sortingOrder}");
                            }
                        }
                    }
                }
            }

            if (foundObjects.Count == 0)
            {
                Debug.LogWarning($"❌ Không tìm thấy object nào tại cell ({gridX}, {gridY})");
            }
            else
            {
                Debug.Log($"\n--- SO SÁNH TỌAĐỘ ---");
                Debug.Log($"Tổng object tìm thấy: {foundObjects.Count}");

                // So sánh từng cặp object
                for (int i = 0; i < foundObjects.Count; i++)
                {
                    for (int j = i + 1; j < foundObjects.Count; j++)
                    {
                        var obj1 = foundObjects[i];
                        var obj2 = foundObjects[j];

                        float deltaX = Mathf.Abs(obj1.worldPos.x - obj2.worldPos.x);
                        float deltaY = Mathf.Abs(obj1.worldPos.y - obj2.worldPos.y);
                        float deltaZ = Mathf.Abs(obj1.worldPos.z - obj2.worldPos.z);

                        Debug.Log($"\n{obj1.type} vs {obj2.type}:");
                        Debug.Log($"   ΔX = {deltaX:F4}, ΔY = {deltaY:F4}, ΔZ = {deltaZ:F4}");

                        if (deltaX > 0.001f || deltaY > 0.001f)
                        {
                            Debug.LogWarning($"   ⚠️  KHÔNG ĐỒNG BỘ! Position khác nhau");
                        }
                        else
                        {
                            Debug.Log($"   ✅ ĐỒNG BỘ! Position giống nhau");
                        }
                    }
                }
            }

            Debug.Log($"\n{'='.ToString().PadRight(80, '=')}");
        }

        /// <summary>
        /// Debug: Kiểm tra vị trí của cùng 1 cell trong 3 layer khác nhau
        /// Dùng: levelLoader.DebugCompare3Layers(5, 5);
        /// </summary>
        public void DebugCompare3Layers(int gridX, int gridY)
        {
            Debug.Log($"\n{'='.ToString().PadRight(70, '=')}\n" +
                      $"[LevelLoader] SO SÁNH 3 LAYER TẠI CELL ({gridX}, {gridY})\n" +
                      $"{'='.ToString().PadRight(70, '=')}");

            Vector3 hazardPos = Vector3.zero;
            Vector3 tablePos = Vector3.zero;
            Vector3 itemPos = Vector3.zero;
            bool foundHazard = false, foundTable = false, foundItem = false;

            // Tìm các object tại cell này
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj == null) continue;

                string name = obj.name;
                // Format: "Type_x_y_Llayer"
                string[] parts = name.Split('_');

                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[1], out int objX) && int.TryParse(parts[2], out int objY))
                    {
                        if (objX == gridX && objY == gridY)
                        {
                            string type = parts[0];

                            if (type == nameof(TileType.Miasma))
                            {
                                hazardPos = obj.transform.position;
                                foundHazard = true;
                                Debug.Log($"\n🔴 LAYER HAZARD:\n   Object: {name}\n   Position: ({hazardPos.x:F3}, {hazardPos.y:F3})");
                            }
                            else if (type == nameof(TileType.Table))
                            {
                                tablePos = obj.transform.position;
                                foundTable = true;
                                Debug.Log($"\n🟡 LAYER TABLE:\n   Object: {name}\n   Position: ({tablePos.x:F3}, {tablePos.y:F3})");
                            }
                            else if (type == nameof(TileType.Lamp) || type == nameof(TileType.Key) ||
                                     type == nameof(TileType.Lighter) || type == nameof(TileType.Flower) ||
                                     type == nameof(TileType.SafePath))
                            {
                                itemPos = obj.transform.position;
                                foundItem = true;
                                Debug.Log($"\n🟢 LAYER ITEM:\n   Object: {name}\n   Position: ({itemPos.x:F3}, {itemPos.y:F3})");
                            }
                        }
                    }
                }
            }

            // So sánh các vị trí
            Debug.Log($"\n--- SỬ SO SÁNH ---");
            if (foundHazard && foundTable)
            {
                float diffY = Mathf.Abs(hazardPos.y - tablePos.y);
                Debug.Log($"Hazard vs Table:");
                Debug.Log($"  ΔY = {diffY:F3}");
                if (diffY > 0.5f)
                    Debug.LogWarning($"  ⚠️  CHÊNH LỆCH Y LỚN!");
                else
                    Debug.Log($"  ✅ Y khớp");
            }

            if (foundHazard && foundItem)
            {
                float diffY = Mathf.Abs(hazardPos.y - itemPos.y);
                Debug.Log($"Hazard vs Item:");
                Debug.Log($"  ΔY = {diffY:F3}");
                if (diffY > 0.5f)
                    Debug.LogWarning($"  ⚠️  CHÊNH LỆCH Y LỚN!");
                else
                    Debug.Log($"  ✅ Y khớp");
            }

            if (foundTable && foundItem)
            {
                float diffY = Mathf.Abs(tablePos.y - itemPos.y);
                Debug.Log($"Table vs Item:");
                Debug.Log($"  ΔY = {diffY:F3}");
                if (diffY > 0.5f)
                    Debug.LogWarning($"  ⚠️  CHÊNH LỆCH Y LỚN!");
                else
                    Debug.Log($"  ✅ Y khớp");
            }

            Debug.Log($"{'='.ToString().PadRight(70, '=')}\n");
        }
        /// <summary>
        /// In ra thông tin tóm tắt các layer đã load
        /// </summary>
        [ContextMenu("Test: Print Layer Summary")]
        public void PrintLayerSummary()
        {
            var layerCount = new Dictionary<string, int>();

            foreach (GameObject obj in spawnedObjects)
            {
                if (obj == null) continue;

                string[] parts = obj.name.Split('_');
                string type = parts.Length > 0 ? parts[0] : "Unknown";

                if (!layerCount.ContainsKey(type))
                    layerCount[type] = 0;
                layerCount[type]++;
            }

            Debug.Log($"\n{'='.ToString().PadRight(50, '=')}");
            Debug.Log($"[LevelLoader] LAYER SUMMARY");
            Debug.Log($"{'='.ToString().PadRight(50, '=')}");
            Debug.Log($"Total Spawned Objects: {spawnedObjects.Count}\n");

            foreach (var kvp in layerCount)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}");
            }

            Debug.Log($"{'='.ToString().PadRight(50, '=')}");
        }

        // 🧲 ENVIRONMENT MANAGEMENT - Các method để kiểm soát environment object
        /// <summary>
        /// Lấy tất cả environment object được sinh ra (Lamp, Flower, Item, Decor, Safe Zone, Exit, v.v...)
        /// </summary>
        public List<GameObject> GetEnvironmentObjects() => new List<GameObject>(environmentObjects);

        /// <summary>
        /// Lấy số lượng environment object hiện tại
        /// </summary>
        public int GetEnvironmentObjectsCount() => environmentObjects.Count;

        /// <summary>
        /// Lấy dictionary của các environment folder (organize by type)
        /// </summary>
        public Dictionary<string, Transform> GetEnvironmentFolders() => new Dictionary<string, Transform>(environmentFolders);

        /// <summary>
        /// Lấy environment object theo loại (Type)
        /// </summary>
        public List<GameObject> GetEnvironmentObjectsByType(string folderName)
        {
            var result = new List<GameObject>();
            if (environmentFolders.ContainsKey(folderName))
            {
                Transform folder = environmentFolders[folderName];
                foreach (Transform child in folder)
                {
                    result.Add(child.gameObject);
                }
            }
            return result;
        }

        public int GetSpawnedObjectCount() => spawnedObjects.Count;
    }

    public class LevelService : Game.Core.Services.ILevelService
    {
        private LevelLoader loader;
        private string currentLevelName;
        public string CurrentLevelName => currentLevelName;
        public event System.Action<string> OnLevelLoaded;
        public event System.Action OnLevelCleared;
        public LevelService(LevelLoader levelLoader) { loader = levelLoader; }
        public void LoadLevel(string levelName) { loader.LoadLevel(levelName); currentLevelName = levelName; OnLevelLoaded?.Invoke(levelName); }
        public void ClearLevel() { loader.ClearCurrentLevel(); currentLevelName = null; OnLevelCleared?.Invoke(); }
    }

    [System.Serializable]
    public class CellBasedLevelData { public string levelID; public int seed; public int generationCount; public int maxLight; public int width; public int height; public Vector2Int spawnPos; public Vector2Int exitPos; public List<CellData> cells = new List<CellData>(); }
    [System.Serializable]
    public class CellData { public int x; public int y; public List<CellObjectData> objects = new List<CellObjectData>(); }
    [System.Serializable]
    public class CellObjectData { public string type; public int layer; }
}