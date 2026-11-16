
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Obracanie monety")]
    public float rotationSpeed = 100f;

    [Header("Wartosc monety")]
    public int coinValue = 1;

    private bool collected = false;

    void Update()
    {
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (CoinManager.Instance == null)
        {
            Debug.LogWarning(
                "Nie znaleziono CoinManager w scenie!"
            );
            return;
        }

        collected = true;

        CoinManager.Instance.AddCoin(coinValue);

        Debug.Log(
            "Zebrano monete! Wartosc: " + coinValue
        );

        gameObject.SetActive(false);
    }
}
