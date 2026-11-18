
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Obracanie monety")]
    public float rotationSpeed = 100f;

    [Header("Unoszenie monety")]
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;

    [Header("Wartosc monety")]
    public int coinValue = 1;

    [Header("Dzwiek zbierania")]
    public AudioClip collectSound;

    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("Efekt zbierania")]
    public GameObject collectEffectPrefab;

    private bool collected = false;

    private Vector3 startPosition;
    private float floatOffset;

    private void Start()
    {
        startPosition = transform.position;

        floatOffset = Random.Range(
            0f,
            Mathf.PI * 2f
        );
    }

    private void Update()
    {
        if (collected)
            return;

        // Obracanie monety
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );

        // Delikatne unoszenie monety
        float newY =
            startPosition.y +
            Mathf.Sin(
                Time.time * floatSpeed + floatOffset
            ) * floatHeight;

        transform.position = new Vector3(
            startPosition.x,
            newY,
            startPosition.z
        );
    }

    private void OnTriggerEnter(Collider other)
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

        // Aktualizacja licznika monet
        CoinManager.Instance.AddCoin(coinValue);

        // Odtworzenie dzwieku
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(
                collectSound,
                transform.position,
                soundVolume
            );
        }

        // Zloty efekt czasteczek
        if (collectEffectPrefab != null)
        {
            Instantiate(
                collectEffectPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        Debug.Log(
            "Zebrano monete! Wartosc: " + coinValue
        );

        // Ukrycie monety
        gameObject.SetActive(false);
    }
}
