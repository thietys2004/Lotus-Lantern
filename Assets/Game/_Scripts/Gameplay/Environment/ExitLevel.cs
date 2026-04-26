using System.Collections;
using UnityEngine;

namespace Gameplay.Environment
{
    public class ExitLevel : MonoBehaviour
    {
        public int nextLevelID;

        private bool isTransitioning = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.CompareTag("Player") && !isTransitioning)
            {

                Game.Gameplay.Player.PlayerController player = collision.GetComponent<Game.Gameplay.Player.PlayerController>();

                if (player != null)
                {

                    if (player.keyCount > 0)
                    {

                        StartCoroutine(HandleLevelComplete(player));
                    }

                }
            }
        }

        private IEnumerator HandleLevelComplete(Game.Gameplay.Player.PlayerController player)
        {
            isTransitioning = true;
            if (Game.Core.AudioManager.Instance != null) Game.Core.AudioManager.Instance.PlayDoorSound();


            player.keyCount--;
            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.UpdateKeyCount(player.keyCount);
            }


            int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (nextLevelID > currentUnlocked)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextLevelID);
                PlayerPrefs.Save();
            }


            yield return new WaitForSeconds(0.5f);


            if (Game.UI.UIManager.Instance != null)
            {
                Game.UI.UIManager.Instance.ShowEndGamePanel(true);
            }
        }
    }
}