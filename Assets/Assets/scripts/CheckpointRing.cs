
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class CheckpointRing : MonoBehaviour
{
    [Header("Ring Settings")]
    public float radius = 1.5f;
    public int segments = 64;
    public float width = 0.08f;

    private void OnEnable()
    {
        CreateRing();
    }

    private void OnValidate()
    {
        CreateRing();
    }

    private void CreateRing()
    {
        LineRenderer line = GetComponent<LineRenderer>();

        if (line == null) return;

        int count = Mathf.Max(3, segments);

        line.useWorldSpace = false;
        line.loop = true;
        line.alignment = LineAlignment.TransformZ;

        line.positionCount = count;

        line.widthCurve =
            AnimationCurve.Constant(0f, 1f, 1f);

        line.widthMultiplier = width;

        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}
