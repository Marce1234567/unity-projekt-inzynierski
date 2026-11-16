
using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Licznik monet")]
    public TMP_Text coinCounterText;

    [Header("Stan monet")]
    public int collectedCoins = 0;
    public int totalCoins = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        collectedCoins = 0;

        // Liczymy wszystkie monety w poziomie.
        Coin[] coins = FindObjectsByType<Coin>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        totalCoins = coins.Length;

        UpdateCoinUI();
    }

    public void AddCoin(int value)
    {
        collectedCoins += value;
        UpdateCoinUI();
    }

    public void UpdateCoinUI()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text =
                "Monety: " + collectedCoins + " / " + totalCoins;
        }
    }

    public int GetCollectedCoins()
    {
        return collectedCoins;
    }

    public int GetTotalCoins()
    {
        return totalCoins;
    }
}
