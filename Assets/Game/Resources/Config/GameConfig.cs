using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Centralized configuration for all game parameters.
    /// Replaces magic numbers throughout the codebase.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private float turnDelay = 0.1f;
        [SerializeField] private float moveTimeout = 0.5f;

        [Header("Pathfinding")]
        [SerializeField] private Vector2 pathClearBoxSize = new Vector2(0.5f, 0.5f);
        [SerializeField] private float safePathCheckRadius = 0.1f;

        [Header("Player Respawn")]
        [SerializeField] private float respawnColliderDisableDelay = 0.2f;
        [SerializeField] private float respawnPositionSetDelay = 0.5f;

        [Header("Player Interactions")]
        [SerializeField] private float lanternSpawnDelay = 0.2f;
        [SerializeField] private float lanternActivateDelay = 0.3f;
        [SerializeField] private float lanternInteractionCooldown = 0.25f;
        [SerializeField] private float interactionDelay = 0.2f;

        [Header("Item Pickup")]
        [SerializeField] private float pickupActivationRadius = 0.3f;
        [SerializeField] private Vector3 lotusSpawnOffset = new Vector3(0f, -0.5f, 0f);

        [Header("Lantern")]
        [SerializeField] private float lanternGridSnapOffset = 0.5f;

        [Header("UI")]
        [SerializeField] private float uiUpdateRate = 0.1f;

        [Header("PCG Level Generation")]
        [SerializeField] private int levelWidth = 13;
        [SerializeField] private int levelHeight = 8;
        [SerializeField] private int maxGenerations = 5000;
        [SerializeField] private int tileEmptyChance = 45;      // 0-45
        [SerializeField] private int tileMiasmaChance = 65;     // 45-65
        [SerializeField] private int tileWallChance = 75;       // 65-75
        [SerializeField] private int tileTableChance = 85;      // 75-85
        [SerializeField] private int tileLampChance = 95;       // 85-95
        [SerializeField] private int maxLampsPerLevel = 3;
        [SerializeField] private int miasmaStepsPerFlower = 3;
        [SerializeField] private int initialFlowerCount = 0;

        // Getters
        public float MoveSpeed => moveSpeed;
        public float GridSize => gridSize;
        public float TurnDelay => turnDelay;
        public float MoveTimeout => moveTimeout;

        public Vector2 PathClearBoxSize => pathClearBoxSize;
        public float SafePathCheckRadius => safePathCheckRadius;

        public float RespawnColliderDisableDelay => respawnColliderDisableDelay;
        public float RespawnPositionSetDelay => respawnPositionSetDelay;

        public float LanternSpawnDelay => lanternSpawnDelay;
        public float LanternActivateDelay => lanternActivateDelay;
        public float LanternInteractionCooldown => lanternInteractionCooldown;
        public float InteractionDelay => interactionDelay;

        public float PickupActivationRadius => pickupActivationRadius;
        public Vector3 LotusSpawnOffset => lotusSpawnOffset;
        public float LanternGridSnapOffset => lanternGridSnapOffset;

        public float UIUpdateRate => uiUpdateRate;

        public int LevelWidth => levelWidth;
        public int LevelHeight => levelHeight;
        public int MaxGenerations => maxGenerations;
        public int TileEmptyChance => tileEmptyChance;
        public int TileMiasmaChance => tileMiasmaChance;
        public int TileWallChance => tileWallChance;
        public int TileTableChance => tileTableChance;
        public int TileLampChance => tileLampChance;
        public int MaxLampsPerLevel => maxLampsPerLevel;
        public int MiasmaStepsPerFlower => miasmaStepsPerFlower;
        public int InitialFlowerCount => initialFlowerCount;

        private static GameConfig _instance;

        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("Config/GameConfig");
                    if (_instance == null)
                    {
                        Debug.LogWarning("GameConfig not found at Resources/Config/GameConfig, searching all Resources for GameConfig...");
                        GameConfig[] configs = Resources.LoadAll<GameConfig>("");
                        if (configs != null && configs.Length > 0)
                        {
                            _instance = configs[0];
                            Debug.LogWarning($"GameConfig fallback loaded from Resources: {configs.Length} found, using {(_instance != null ? _instance.name : "null")}");
                        }
                        else
                        {
                            Debug.LogError("GameConfig not found in Resources/Config/ folder!");
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
