using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAndLevelManager : MonoBehaviour
{
    // NEW: Global instance lets your laser script talk directly to this script
    public static GameAndLevelManager Instance;

    [Header("Alien Management")]
    [Tooltip("Drag all the aliens placed in your scene into this list!")]
    [SerializeField] private List<GameObject> alienPool = new List<GameObject>();
    [SerializeField] private float delayBetweenAliens = 1.0f;

    [Header("Level Pad Setup")]
    [SerializeField] private GameObject levelPad;
    [SerializeField] private string padTag = "LevelPad";
    [SerializeField] private float delayBeforePadAppears = 1.5f;

    private GameObject currentActiveAlien;
    private int currentAlienIndex = 0;
    private MeshRenderer padMeshRenderer;
    private Collider padCollider;
    private bool isLevelTransitioning = false;
    private bool padHasAppeared = false;
    private bool isWaitingToSpawn = false; // FIX: Prevents Coroutine spam cleanly without breaking null checks

    void Awake()
    {
        // Set up the static reference so the Laser can find this manager instantly
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
{
    // 1. Set up the Level Pad (Hide it at start)
    if (levelPad != null)
    {
        padMeshRenderer = levelPad.GetComponent<MeshRenderer>();
        padCollider = levelPad.GetComponent<Collider>();
        if (padMeshRenderer != null) padMeshRenderer.enabled = false;
        if (padCollider != null) padCollider.enabled = false;
    }

    // NEW FIX: Run the initialization safely through a Coroutine to prevent race conditions
    StartCoroutine(SafeInitializeGame());
}

private IEnumerator SafeInitializeGame()
{
    // Wait exactly one frame for all VR systems and alien Awake() functions to complete
    yield return null; 

    // --- TRULY RANDOMIZE THE ALIEN ORDER ---
    for (int i = 0; i < alienPool.Count; i++)
    {
        GameObject temp = alienPool[i];
        int randomIndex = Random.Range(i, alienPool.Count);
        alienPool[i] = alienPool[randomIndex];
        alienPool[randomIndex] = temp;
    }

    // 2. Hide ALL aliens in the scene safely now that they are awake
    foreach (GameObject alien in alienPool)
    {
        if (alien != null)
        {
            SetAlienVisibility(alien, false);
        }
    }

    // 3. Reveal the very first alien to start the game
    RevealNextAlien();
}


    // NEW & CRITICAL FIX: The Laser script calls this function directly when it kills an alien.
    // This removes the need to constantly check "if (currentActiveAlien == null)" in Update.
    public void ReportAlienDeath(GameObject destroyedAlien)
    {
        if (padHasAppeared || isWaitingToSpawn) return;

        // Double check to make sure the dead object matches what we currently have active
        if (destroyedAlien == currentActiveAlien || currentActiveAlien == null)
        {
            currentAlienIndex++;

            if (currentAlienIndex < alienPool.Count)
            {
                StartCoroutine(WaitAndSpawnNext());
            }
            else
            {
                // Absolute final alien has been destroyed! Bring out the portal pad.
                StartCoroutine(RevealLevelPad());
            }
        }
    }

    private void RevealNextAlien()
    {
        if (currentAlienIndex < alienPool.Count)
        {
            currentActiveAlien = alienPool[currentAlienIndex];
            if (currentActiveAlien != null)
            {
                SetAlienVisibility(currentActiveAlien, true);
                Debug.Log($"Alien {currentAlienIndex + 1} of {alienPool.Count} has appeared!");
            }
        }
    }

    private IEnumerator WaitAndSpawnNext()
    {
        isWaitingToSpawn = true;
        currentActiveAlien = null; // Cleanly clear out the reference 
        
        yield return new WaitForSeconds(delayBetweenAliens);
        
        isWaitingToSpawn = false;
        RevealNextAlien();
    }

    // Helper function that safely toggles an alien's visibility on/off
    private void SetAlienVisibility(GameObject alien, bool visible)
    {
        Renderer r = alien.GetComponent<Renderer>();
        if (r != null) r.enabled = visible;

        // Also toggle child objects (in case your alien has separate armor/eyes meshes)
        Renderer[] childRenderers = alien.GetComponentsInChildren<Renderer>();
        foreach (Renderer cr in childRenderers) cr.enabled = visible;
    }

    private IEnumerator RevealLevelPad()
    {
        padHasAppeared = true;
        yield return new WaitForSeconds(delayBeforePadAppears);
        if (padMeshRenderer != null) padMeshRenderer.enabled = true;
        if (padCollider != null) padCollider.enabled = true;
        Debug.Log("All finite aliens destroyed! The Level Pad is now visible!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (padHasAppeared && !isLevelTransitioning && other.CompareTag(padTag))
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadNextLevel()
    {
        isLevelTransitioning = true;
        yield return new WaitForSeconds(0.5f);
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
