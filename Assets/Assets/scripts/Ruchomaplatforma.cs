using UnityEngine;

public class Ruchomaplatforma : MonoBehaviour
{
    public float speed = 2f;

    public bool moveUpDown = true;
    public float height = 3f;

    public bool moveForwardBack = false;
    public float distance = 3f;

    private Vector3 startPos;
    private Vector3 lastPosition;

    public Vector3 PlatformDelta { get; private set; }

    void Start()
    {
        startPos = transform.position;
        lastPosition = transform.position;
    }

    void Update()
    {
        float y = 0f;
        float z = 0f;

        if (moveUpDown)
            y = Mathf.Sin(Time.time * speed) * height;

        if (moveForwardBack)
            z = Mathf.Sin(Time.time * speed) * distance;

        transform.position = new Vector3(
            startPos.x,
            startPos.y + y,
            startPos.z + z
        );

        PlatformDelta = transform.position - lastPosition;
        lastPosition = transform.position;
    }
}