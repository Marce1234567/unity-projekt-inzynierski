using UnityEngine;

public class FinishTextAnimation : MonoBehaviour
{
    [Header("Unoszenie napisu")]
    public float floatHeight = 0.15f;
    public float floatSpeed = 2f;

    [Header("Pulsowanie napisu")]
    public bool pulseEnabled = true;
    public float pulseAmount = 0.05f;
    public float pulseSpeed = 2f;

    private Vector3 startPosition;
    private Vector3 startScale;

    void Start()
    {
        startPosition = transform.localPosition;
        startScale = transform.localScale;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * floatSpeed)
            * floatHeight;

        transform.localPosition = startPosition
            + Vector3.up * offsetY;

        if (pulseEnabled)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed)
                * pulseAmount;

            transform.localScale = startScale * pulse;
        }
    }
}