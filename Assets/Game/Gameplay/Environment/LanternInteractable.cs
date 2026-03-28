using System.Collections.Generic;
using UnityEngine;
namespace Game.Gameplay.Environment
{
    public class LanternInteractable : MonoBehaviour
    {

        [Header("Cài đặt Đèn")]
        public bool isOn = false;
        public float gridSize = 1f;

        [Header("Prefab & Tham chiếu")]
        public GameObject safePathPrefab;
        private Animator anim;


        private List<GameObject> spawnedTiles = new List<GameObject>();

        void Awake()
        {
            anim = GetComponent<Animator>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (anim != null) anim.SetBool("IsOn", isOn);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void ToggleLantern()
        {
            isOn = !isOn;

            if (anim != null) anim.SetBool("IsOn", isOn);

            if (isOn)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }
        private void TurnOn()
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {

                    if (x == 0 && y == 0) continue;


                    Vector3 rawPos = transform.position + new Vector3(x, y, 0f) * gridSize;
                    float snapX = Mathf.Floor(rawPos.x) + 0.5f;
                    float snapY = Mathf.Floor(rawPos.y) + 0.5f;
                    Vector3 snappedPos = new Vector3(snapX, snapY, 0f);

                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(snappedPos, 0.1f);
                    bool isAlreadyLit = false;

                    foreach (Collider2D col in hitColliders)
                    {

                        if (col.CompareTag("SafePath"))
                        {
                            isAlreadyLit = true;
                            break;
                        }
                    }


                    if (!isAlreadyLit)
                    {
                        GameObject tile = Instantiate(safePathPrefab, snappedPos, Quaternion.identity);
                        spawnedTiles.Add(tile);
                    }
                }
            }
        }
        private void TurnOff()
        {
            foreach (GameObject tile in spawnedTiles)
            {
                if (tile != null) Destroy(tile);
            }
            spawnedTiles.Clear();
        }
    }
}

