using Game.Gameplay.Player;
using UnityEngine;
namespace Game.Gameplay.Environment
{
    public class StoneAltar : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;
        private bool isActivated = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated) return;

            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();

                if (player != null)
                {
                    ActivateCheckpoint(player);

                }
            }
        }

        private void ActivateCheckpoint(PlayerController player)
        {
            isActivated = true;
            Vector3 offset = new Vector3(1f, 0, 0);
            player.SetRespawnPoint(transform.position + offset);
        }
    }
}
