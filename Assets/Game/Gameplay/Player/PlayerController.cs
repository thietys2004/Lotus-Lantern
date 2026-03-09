using System.Collections;
using UnityEngine;

namespace Game.Gameplay.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float gridSize = 1f;

        private SpriteRenderer sr;
        private Rigidbody2D rb;
        private Animator playeranimator;

        [Header("Respawn Settings")]
        public Vector3 respawnPosition;

        private bool isMoving = false;
        private bool isRespawning = false;


        private float lastX = 0f;
        private float lastY = -1f;
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

        }
        private void HandleInput()
        {
            if (!isMoving && !isRespawning)
            {
                float inputX = Input.GetAxisRaw("Horizontal");
                float inputY = Input.GetAxisRaw("Vertical");

                if (inputX != 0)
                {
                    inputY = 0;
                }

                if (inputX != 0 || inputY != 0)
                {

                    lastX = inputX;
                    lastY = inputY;


                    if (inputX > 0)
                        sr.flipX = false;
                    else if (inputX < 0)
                        sr.flipX = true;

                    Vector3 targetPos = transform.position + new Vector3(inputX, inputY, 0f) * gridSize;
                    StartCoroutine(MoveToGrid(targetPos));
                }
            }
        }
        private IEnumerator MoveToGrid(Vector3 targetPos)
        {
            isMoving = true;

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
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

            Debug.Log("Nhân vật đã hồi sinh tại: " + safePosition);

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
    }
}