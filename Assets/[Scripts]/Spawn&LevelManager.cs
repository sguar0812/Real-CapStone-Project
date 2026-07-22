using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAndLevelManager : MonoBehaviour
{
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

        // --- NEW: TRULY RANDOMIZE THE ALIEN ORDER ---
        // This loops through your list and randomly shuffles their positions
        for (int i = 0; i < alienPool.Count; i++)
        {
            GameObject temp = alienPool[i];
            int randomIndex = Random.Range(i, alienPool.Count);
            alienPool[i] = alienPool[randomIndex];
            alienPool[randomIndex] = temp;
        }
        // --------------------------------------------

        // 2. Hide ALL aliens in the scene at the absolute start
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


    void Update()
    {
        if (padHasAppeared) return;

        // If the current alien was destroyed (is null) and we haven't run out of aliens yet
        if (currentActiveAlien == null && currentAlienIndex < alienPool.Count)
        {
            // Move to the next slot in our finite list
            currentAlienIndex++;

            if (currentAlienIndex < alienPool.Count)
            {
                // Start a timed delay before showing the next random alien
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
        // Prevent the update loop from spamming this while waiting
        currentActiveAlien = this.gameObject; 

        yield return new WaitForSeconds(delayBetweenAliens);
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
