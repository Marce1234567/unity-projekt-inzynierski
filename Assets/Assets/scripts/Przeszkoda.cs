using UnityEngine;

public class Przeszkoda : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerLife playerLife = other.GetComponentInParent<PlayerLife>();

        if (playerLife != null)
        {
            playerLife.KillPlayer();
        }
    }
}
