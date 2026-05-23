using System.Collections;
using UnityEngine;

public class LoadSavedPosition : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;

        int slotToLoad = PlayerPrefs.GetInt("SlotToLoad", -1);

        if (slotToLoad == -1)
            yield break;

        if (!SaveSystem.SaveExists(slotToLoad))
            yield break;

        Rigidbody rb = GetComponent<Rigidbody>();
        PlayerController controller = GetComponent<PlayerController>();
        PlayerLife playerLife = GetComponent<PlayerLife>();

        if (controller != null)
            controller.isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Vector3 savedPosition =
            SaveSystem.GetSavedPosition(slotToLoad);

        Quaternion savedRotation =
            SaveSystem.GetSavedRotation(slotToLoad);

        transform.position = savedPosition;
        transform.rotation = savedRotation;

        if (playerLife != null)
        {
            playerLife.SetCheckpoint(
                savedPosition,
                savedRotation
            );
        }

        yield return null;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        if (controller != null)
            controller.isDead = false;

        PlayerPrefs.SetInt("SlotToLoad", -1);
        PlayerPrefs.Save();
    }
}