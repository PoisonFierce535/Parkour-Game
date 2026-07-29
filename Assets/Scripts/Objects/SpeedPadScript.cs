using System.Collections;
using UnityEngine;

public class SpeedPadScript : MonoBehaviour
{
    public float speedBoost = 30f;

    // EDITABLE //
    private static readonly WaitForSeconds RESET_BOOST_DURATION = new(1f);
    // EDITABLE //

    private bool canBoost = true;



    private void OnTriggerEnter(Collider other)
    {
        if (canBoost && other.gameObject.CompareTag("Player"))
        {
            canBoost = false;

            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            Vector3 dir = rb.linearVelocity.normalized;

            rb.linearVelocity += dir * speedBoost;

            StartCoroutine(ResetBoost());
        }
    }

    IEnumerator ResetBoost()
    {
        yield return RESET_BOOST_DURATION;
        canBoost = true;
    }
}
