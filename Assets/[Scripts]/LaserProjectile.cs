using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Search the object we hit (or any parent above it) for the AlienHealth script
        AlienHealth alien = collision.gameObject.GetComponentInParent<AlienHealth>();

        if (alien != null)
        {
            // Trigger the alien's death sequence
            alien.KillAlien();

            // Destroy the laser bullet
            Destroy(gameObject);
        }
    }

    // Include this too, just in case your laser is set up as a Trigger instead of a Collision
    private void OnTriggerEnter(Collider other)
    {
        AlienHealth alien = other.GetComponentInParent<AlienHealth>();

        if (alien != null)
        {
            alien.KillAlien();
            Destroy(gameObject);
        }
    }
}
