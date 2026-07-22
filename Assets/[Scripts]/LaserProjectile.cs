using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    // Automatically runs the exact millisecond this laser hits another physics object
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object we struck has the "Alien" tag applied to it
        if (collision.gameObject.CompareTag("Alien"))
        {
            // 1. COMPLETELY delete the alien object out of the world
            // This forces the Master Manager script to notice it's dead
            Destroy(collision.gameObject); 
            
            // 2. Instantly delete the laser bullet itself so it doesn't fly forever
            Destroy(gameObject); 
        }
    }
}
