using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        ExecuteAlienDeath(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        ExecuteAlienDeath(other.gameObject);
    }

    private void ExecuteAlienDeath(GameObject hitObject)
    {
        // Look up the parent chain to find the AlienHealth script safely
        AlienHealth alien = hitObject.GetComponentInParent<AlienHealth>();

        if (alien != null)
        {
            alien.KillAlien();
            Destroy(gameObject); // Clear out the laser bullet
        }
    }
}
