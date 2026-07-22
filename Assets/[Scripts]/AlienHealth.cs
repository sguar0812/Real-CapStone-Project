using UnityEngine;

public class AlienHealth : MonoBehaviour
{
    // Any laser script can call this function to safely kill the alien
    public void KillAlien()
    {
        // 1. Report directly to the manager that THIS exact parent object is dead
        if (SpawnLevelManager.Instance != null)
        {
            SpawnLevelManager.Instance.ReportAlienDeath(this.gameObject);
        }
        else
        {
            Debug.LogError("Manager instance is missing! Make sure GameAndLevelManager is in the scene.");
        }

        // 2. Destroy this entire top-level object
        Destroy(this.gameObject);
    }
}
