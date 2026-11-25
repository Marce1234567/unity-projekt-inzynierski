
using UnityEngine;

public class CheckpointBillboard : MonoBehaviour
{
    [Header("Floating Animation")]
    public float floatHeight = 0.15f;
    public float floatSpeed = 2f;

    private Transform cameraTransform;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        // Unoszenie napisu
        transform.localPosition = startPosition +
            Vector3.up * Mathf.Sin(Time.time * floatSpeed)
            * floatHeight;

        // Obracanie napisu w strone kamery
        if (cameraTransform != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - cameraTransform.position,
                Vector3.up
            );
        }
    }
}
