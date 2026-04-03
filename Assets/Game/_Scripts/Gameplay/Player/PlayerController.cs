using System.Collections;
using UnityEngine;

namespace Game.Gameplay.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] public float moveSpeed = 5f;
        [SerializeField] public float gridSize = 1f;
        [SerializeField] private float turnDelay = 0.1f;

        private SpriteRenderer sr;
        private Rigidbody2D rb;
        private Animator playeranimator;

        [Header("Respawn Settings")]
        public Vector3 respawnPosition;

        [Header("Lantern Settings")]
        public GameObject lotusPrefab;
        private GameObject currentLotus;
        private bool isInteracting = false;

        private bool isMoving = false;
        private bool isRespawning = false;
        private bool isTurning = false;


        private float lastX = 0f;
        private float lastY = -1f;

        [Header("Tương tác Môi trường")]
        private Game.Gameplay.Environment.LanternInteractable nearbyLantern = null;
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            playeranimator = GetComponent<Animator>();
        }

        void Start()
        {
            respawnPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            HandleInput();
            UpdateAnimator();


            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnPlaceLanternButtonPressed();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInteractEnvironmentButtonPressed();
            }

        }
        private void HandleInput()
        {
            if (!isMoving && !isRespawning && !isInteracting && !isTurning)
            {
                float inputX = Input.GetAxisRaw("Horizontal");
                float inputY = Input.GetAxisRaw("Vertical");

                if (inputX != 0)
                {
                    inputY = 0;
                }

                if (inputX != 0 || inputY != 0)
                {
                    if (inputX != lastX || inputY != lastY)
                    {


                        lastX = inputX;
                        lastY = inputY;
                        if (lastX > 0) sr.flipX = false;
                        else if (lastX < 0) sr.flipX = true;
                        UpdateAnimator();
                        StartCoroutine(TurnCooldown());
                    }


                    //if (inputX > 0)
                    //    sr.flipX = false;
                    //else if (inputX < 0)
                    //    sr.flipX = true;
                    else
                    {
                        Vector3 targetPos = transform.position + new Vector3(inputX, inputY, 0f) * gridSize;
                        if (IsPathClear(targetPos))
                        {
                            StartCoroutine(MoveToGrid(targetPos));
                        }

                    }
                }
            }
        }
        private IEnumerator TurnCooldown()
        {
            isTurning = true;

            yield return new WaitForSeconds(turnDelay);
            isTurning = false;
        }
        private bool IsPathClear(Vector3 targetPos)
        {
            Collider2D[] hitCollider = Physics2D.OverlapCircleAll(targetPos, 0.4f);
            foreach (Collider2D col in hitCollider)
            {
                if (col.gameObject == this.gameObject) continue;
                if (!col.isTrigger)
                {
                    return false;
                }
            }
            return true;
        }
        public bool IsSafePath()
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);

            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag("SafePath"))
                {
                    return true;
                }
            }
            return false;
        }
        private IEnumerator MoveToGrid(Vector3 targetPos)
        {
            isMoving = true;

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.AddStep();
            }
            transform.position = targetPos;
            isMoving = false;
        }
        public void Die()
        {
            if (isRespawning) return;


            if (Game.Core.SoulFireManager.Instance != null)
            {
                Game.Core.SoulFireManager.Instance.ClearAllLamps();
            }
            StopAllCoroutines();
            isMoving = false;

            StartCoroutine(RespawnRoutine());
        }
        private IEnumerator RespawnRoutine()
        {
            isRespawning = true;
            Collider2D col = GetComponent<Collider2D>();
            col.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(0.2f);


            Vector3 safePosition = new Vector3(respawnPosition.x, respawnPosition.y, 0f);
            transform.position = safePosition;



            yield return new WaitForSeconds(0.5f);
            col.enabled = true;
            isRespawning = false;
        }
        public void SetRespawnPoint(Vector3 newPoint)
        {
            respawnPosition = new Vector3(newPoint.x, newPoint.y, 0f);
        }

        private void UpdateAnimator()
        {


            playeranimator.SetFloat("moveX", Mathf.Abs(lastX));
            playeranimator.SetFloat("moveY", lastY);
            playeranimator.SetBool("IsMoving", isMoving);
        }

        public void OnPlaceLanternButtonPressed()
        {

            if (!isMoving && !isRespawning && !isInteracting)
            {
                StartCoroutine(SpawnLotusRoutine());
            }
        }
        private IEnumerator SpawnLotusRoutine()
        {
            isInteracting = true;



            playeranimator.SetBool("IsInteracting", true);


            yield return new WaitForSeconds(0.2f);


            if (currentLotus != null)
            {
                Destroy(currentLotus);
            }


            Vector3 rawFeetPos = transform.position + new Vector3(0f, -0.5f, 0f);
            float snapX = Mathf.Floor(rawFeetPos.x) + 0.5f;
            float snapY = Mathf.Floor(rawFeetPos.y) + 0.5f;
            Vector3 snappedPos = new Vector3(snapX, snapY, 0f);
            currentLotus = Instantiate(lotusPrefab, snappedPos, Quaternion.identity);




            Game.Gameplay.Skill.LotusLantern lantern = currentLotus.GetComponent<Game.Gameplay.Skill.LotusLantern>();
            if (lantern != null)
            {

                lantern.ActivateLantern(new Vector2(lastX, lastY));
            }


            yield return new WaitForSeconds(0.3f);

            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.AddStep();
            }
            playeranimator.SetBool("IsInteracting", false);
            isInteracting = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("InteractableLantern"))
            {

                nearbyLantern = collision.GetComponent<Game.Gameplay.Environment.LanternInteractable>();

            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("InteractableLantern"))
            {
                nearbyLantern = null;

            }
        }
        public void OnInteractEnvironmentButtonPressed()
        {

            if (!isMoving && !isRespawning && !isInteracting && nearbyLantern != null)
            {
                if (Game.UI.UIManager.Instance != null)
                {
                    Game.UI.UIManager.Instance.AddStep();
                }
                nearbyLantern.ToggleLantern();

            }
        }
    }
}