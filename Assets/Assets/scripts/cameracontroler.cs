using UnityEngine;

public class cameracontroler : MonoBehaviour
{
    public Transform target;

    [Header("Distance")]
    public float distance = 6.5f;
    public float height = 1.2f;

    [Header("Mouse")]
    public float mouseSensitivity = 3f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("Smooth")]
    public float smoothSpeed = 10f;

    private float yaw = 0f;
    private float pitch = 10f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(targetPosition);
    }
}
