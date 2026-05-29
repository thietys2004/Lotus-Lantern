using UnityEngine;

namespace Game.Gameplay.Player.Components
{
    /// <summary>
    /// Handles player animation state management.
    /// Decoupled from movement and interaction logic.
    /// </summary>
    public class PlayerAnimationComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        private static readonly int MoveXHash = Animator.StringToHash("moveX");
        private static readonly int MoveYHash = Animator.StringToHash("moveY");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsInteractingHash = Animator.StringToHash("IsInteracting");

        private void Start()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (animator == null)
                animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Update animation based on movement direction and state.
        /// </summary>
        public void UpdateMovementAnimation(float directionX, float directionY, bool isMoving)
        {
            animator.SetFloat(MoveXHash, Mathf.Abs(directionX));
            animator.SetFloat(MoveYHash, directionY);
            animator.SetBool(IsMovingHash, isMoving);
        }

        /// <summary>
        /// Flip sprite based on horizontal direction.
        /// </summary>
        public void FaceDirection(float directionX)
        {
            if (directionX > 0)
                spriteRenderer.flipX = false;
            else if (directionX < 0)
                spriteRenderer.flipX = true;
        }

        /// <summary>
        /// Set interaction animation state.
        /// </summary>
        public void SetInteracting(bool isInteracting)
        {
            animator.SetBool(IsInteractingHash, isInteracting);
        }

        /// <summary>
        /// Play a specific animation clip.
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            animator.SetTrigger(Animator.StringToHash(animationName));
        }

        /// <summary>
        /// Get current sprite flip state.
        /// </summary>
        public bool IsFacingLeft => spriteRenderer.flipX;

        /// <summary>
        /// Reset animation to idle state.
        /// </summary>
        public void ResetToIdle()
        {
            animator.SetFloat(MoveXHash, 0);
            animator.SetFloat(MoveYHash, 0);
            animator.SetBool(IsMovingHash, false);
            animator.SetBool(IsInteractingHash, false);
        }
    }
}
