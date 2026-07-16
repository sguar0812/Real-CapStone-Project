using System.Collections;
using System.Collections.Generic;
using UnityEngine;




using UnityEngine; //  CORRECT: All using clauses at the very top
using System.Collections;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Alien"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}

public class DestroyOnCollision : MonoBehaviour
{
    // Automatically runs when another collider physically hits this object
    private void OnCollisionEnter(Collision collision)
    {
        // Optional: Check if the hitting object has a specific tag (e.g., "Alien")
        if (collision.gameObject.CompareTag("Alien"))
        {
            Destroy(gameObject); // Destroys this object
        }
    }
}

public class Dart : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object we hit has the tag "Alien"
        if (collision.gameObject.CompareTag("Alien"))
        {
            Destroy(collision.gameObject); // Destroys the target object
            Destroy(gameObject);           // Destroys the bullet itself so it doesn't fly forever
        }
    }
}