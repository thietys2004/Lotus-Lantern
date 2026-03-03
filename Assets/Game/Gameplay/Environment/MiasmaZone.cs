using Game.Gameplay.Player;
using UnityEngine;

public class MiasmaZone : MonoBehaviour
{
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
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("Lạc vào Chướng Khí!");
                // Gọi hàm Die() trong PlayerController
                player.Die();
            }
        }
    }
}
