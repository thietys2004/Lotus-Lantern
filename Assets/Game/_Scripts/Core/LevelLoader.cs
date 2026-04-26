using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    public class LevelLoader : MonoBehaviour
    {
        public Transform gridParent;

        [Header(" Prefabs")]
        public GameObject playerPrefab;
        public GameObject wallPrefab;
        public GameObject hazardPrefab; // Miasma
        public GameObject tablePrefab;
        public GameObject lampPrefab;
        public GameObject lighterPrefab;
        public GameObject keyPrefab;
        public GameObject exitPrefab;
        public GameObject flowerPrefab;

        private List<GameObject> spawnedObjects = new List<GameObject>();


        public void LoadLevel(string fileName)
        {
            // 1. Đọc file từ thư mục Data
            string filePath = Path.Combine(Application.dataPath, "Game/_Scripts/Data/Levels", fileName + ".json");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Không tìm thấy file map: {filePath}");
                return;
            }

            string jsonText = File.ReadAllText(filePath);
            LevelData data = JsonUtility.FromJson<LevelData>(jsonText);


            ClearCurrentLevel();


            foreach (TileData tile in data.tiles)
            {
                GameObject prefab = GetPrefab(tile.type);
                if (prefab != null)
                {
                    Vector3 pos = new Vector3(tile.x, tile.y, 0);
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity);


                    OrganizeObject(go, tile.type);
                    spawnedObjects.Add(go);
                }
            }


            if (playerPrefab != null)
            {
                Vector3 spawnPos = new Vector3(data.spawnPos.x, data.spawnPos.y, 0);

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                    player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                else
                    player.transform.position = spawnPos;
            }

            Debug.Log($"<color=cyan>Đã tải xong {fileName}. Max Light: {data.maxLight}</color>");
        }

        private GameObject GetPrefab(string type)
        {
            switch (type)
            {
                case "Wall": return wallPrefab;
                case "Miasma": return hazardPrefab;
                case "Table": return tablePrefab;
                case "Lamp": return lampPrefab;
                case "Lighter": return lighterPrefab;
                case "Key": return keyPrefab;
                case "ExitDoor": return exitPrefab;
                case "Flower": return flowerPrefab;
                default: return null;
            }
        }

        private void OrganizeObject(GameObject go, string type)
        {
            if (gridParent == null) return;
            // Tìm thư mục con tương ứng (Wall, Hazard, etc.)
            string folderName = type;
            if (type == "Miasma") folderName = "Hazard";

            Transform folder = gridParent.Find(folderName);
            if (folder != null) go.transform.SetParent(folder);
            else go.transform.SetParent(gridParent);
        }

        public void ClearCurrentLevel()
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null) DestroyImmediate(obj);
            }
            spawnedObjects.Clear();
        }
    }
}

