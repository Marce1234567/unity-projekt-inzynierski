using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody))]
public class Ruchomaplatforma : MonoBehaviour
{
    [Header("Ruch platformy")]
    public float speed = 2f;

    public bool moveUpDown = true;
    public float height = 3f;

    public bool moveForwardBack = false;
    public float distance = 3f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Vector3 lastPosition;
    private float movementTime;

    public Vector3 PlatformDelta { get; private set; }
    public Vector3 PlatformVelocity { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void Start()
    {
        startPos = rb.position;
        lastPosition = rb.position;
    }

    private void FixedUpdate()
    {
        movementTime += Time.fixedDeltaTime;

        float y = moveUpDown ? Mathf.Sin(movementTime * speed) * height : 0f;
        float z = moveForwardBack ? Mathf.Sin(movementTime * speed) * distance : 0f;

        Vector3 targetPosition = new Vector3(
            startPos.x,
            startPos.y + y,
            startPos.z + z
        );

        PlatformDelta = targetPosition - lastPosition;
        PlatformVelocity = PlatformDelta / Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
        lastPosition = targetPosition;
    }
}
