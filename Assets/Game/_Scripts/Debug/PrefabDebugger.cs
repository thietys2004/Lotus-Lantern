using UnityEngine;

public class PrefabDebugger : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject miasmaHazardPrefab;
    public GameObject tablePrefab;
    public GameObject lampPrefab;
    public GameObject lighterPrefab;
    public GameObject keyPrefab;
    public GameObject exitDoorPrefab;
    public GameObject flowerPrefab;
    public GameObject safePathPrefab;

    void Start()
    {
        Debug.Log("=== PREFAB STRUCTURE DEBUG ===");
        DebugPrefab("Wall", wallPrefab);
        DebugPrefab("Miasma", miasmaHazardPrefab);
        DebugPrefab("Table", tablePrefab);
        DebugPrefab("Lamp", lampPrefab);
        DebugPrefab("Lighter", lighterPrefab);
        DebugPrefab("Key", keyPrefab);
        DebugPrefab("ExitDoor", exitDoorPrefab);
        DebugPrefab("Flower", flowerPrefab);
        DebugPrefab("SafePath", safePathPrefab);
    }

    void DebugPrefab(string name, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"❌ {name} prefab is NULL!");
            return;
        }

        Debug.Log($"\n📦 {name}:");

        Rigidbody2D rb = prefab.GetComponent<Rigidbody2D>();
        Debug.Log($"  - Main Rigidbody2D: {(rb != null ? $"✓ ({rb.bodyType})" : "❌ MISSING")}");

        Rigidbody2D[] allRbs = prefab.GetComponentsInChildren<Rigidbody2D>();
        Debug.Log($"  - Total Rigidbody2D (incl. children): {allRbs.Length}");
        foreach (Rigidbody2D childRb in allRbs)
        {
            Debug.Log($"    • {childRb.gameObject.name}: bodyType={childRb.bodyType}");
        }

        Collider2D[] colliders = prefab.GetComponentsInChildren<Collider2D>();
        Debug.Log($"  - Total Collider2D: {colliders.Length}");
        foreach (Collider2D col in colliders)
        {
            Debug.Log($"    • {col.gameObject.name}: {col.GetType().Name} (trigger={col.isTrigger}, enabled={col.enabled})");
        }

        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
        Debug.Log($"  - SpriteRenderer: {(sr != null ? "✓" : "❌ MISSING")}");
    }
}
