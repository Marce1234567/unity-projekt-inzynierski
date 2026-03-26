using UnityEngine;

public class Levelfinish : MonoBehaviour
{
    public GameObject levelCompletePanel;
    private bool finished = false;

    void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!finished && other.CompareTag("Player"))
        {
            finished = true;
            levelCompletePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
