using UnityEngine;

public class FinishLineGenerator : MonoBehaviour
{
    [Header("Wymiary szachownicy")]
    public int columns = 20;
    public int rows = 3;
    public float tileSize = 0.45f;

    [Header("Kolory")]
    public Color whiteColor = Color.white;
    public Color blackColor = Color.black;

    private Material whiteMaterial;
    private Material blackMaterial;

    void Start()
    {
        GenerateFinishLine();
    }

    void GenerateFinishLine()
    {
        Shader shader = Shader.Find("Standard");

        whiteMaterial = new Material(shader);
        whiteMaterial.color = whiteColor;

        blackMaterial = new Material(shader);
        blackMaterial.color = blackColor;

        float offsetX = (columns - 1) * tileSize / 2f;
        float offsetZ = (rows - 1) * tileSize / 2f;

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                GameObject tile = GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

                tile.name = "FinishTile_" + x + "_" + z;

                tile.transform.SetParent(transform, false);

                tile.transform.localPosition = new Vector3(
                    x * tileSize - offsetX,
                    0f,
                    z * tileSize - offsetZ
                );

                tile.transform.localScale = new Vector3(
                    tileSize,
                    0.02f,
                    tileSize
                );

                Collider col = tile.GetComponent<Collider>();
                col.enabled = false;
                Destroy(col);

                Renderer rend = tile.GetComponent<Renderer>();

                rend.sharedMaterial = (x + z) % 2 == 0
                    ? whiteMaterial
                    : blackMaterial;

                rend.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;

                rend.receiveShadows = false;
            }
        }
    }

    void OnDestroy()
    {
        if (whiteMaterial != null)
            Destroy(whiteMaterial);

        if (blackMaterial != null)
            Destroy(blackMaterial);
    }
}