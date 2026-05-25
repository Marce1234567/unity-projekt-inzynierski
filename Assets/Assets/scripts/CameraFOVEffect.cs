using UnityEngine;

public class CameraFOVEffect : MonoBehaviour
{
    public Camera cam;
    public float normalFOV = 60f;
    public float dashFOV = 75f;
    public float changeSpeed = 8f;

    private float targetFOV;

    void Start()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        targetFOV = normalFOV;

        if (cam != null)
            cam.fieldOfView = normalFOV;
    }

    void Update()
    {
        if (cam == null) return;

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            Time.deltaTime * changeSpeed
        );
    }

    public void DashFOV()
    {
        targetFOV = dashFOV;
    }

    public void NormalFOV()
    {
        targetFOV = normalFOV;
    }
}