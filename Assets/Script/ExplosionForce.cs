using System.Collections;
using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    public float explosionRadius = 5f; // Radius of explosion
    public float explosionForce = 700f; // Force of explosion
    public float upwardsModifier = 1f; // Lifts objects upwards

    public LayerMask affectedLayer; // Only affect objects on this layer
    private AudioSource source;
    void Awake()
    {
        source = GetComponent<AudioSource>();
        TriggerExplosion();
    }

    private IEnumerator WaitSeconds()
    {
        yield return new WaitForSeconds(0.2f);
;
    }

    private void LateUpdate()
    {
        if (!source.isPlaying)
        {
            Destroy(gameObject);
        }
    }
    void TriggerExplosion()
    {
        // Get all nearby colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, affectedLayer);

        // Apply explosion force to each collider with a Rigidbody
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Apply explosion force
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier);
            }
        }
    }

    // Optional: To draw the explosion radius in the editor for visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
