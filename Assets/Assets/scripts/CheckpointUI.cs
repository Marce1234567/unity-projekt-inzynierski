using System.Collections;
using UnityEngine;

public class CheckpointUI : MonoBehaviour
{
    public GameObject checkpointText;
    public float showTime = 2f;

    private Coroutine currentRoutine;

    public void ShowCheckpoint()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowCheckpointRoutine());
    }

    private IEnumerator ShowCheckpointRoutine()
    {
        checkpointText.SetActive(true);
        yield return new WaitForSeconds(showTime);
        checkpointText.SetActive(false);
        currentRoutine = null;
    }
}