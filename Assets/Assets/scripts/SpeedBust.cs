using UnityEngine;

public class SpeedBust : MonoBehaviour
{
    public float boostForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(other.transform.forward * boostForce, ForceMode.Impulse);
        }
    }
}
