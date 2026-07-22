using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class SpawnLevelManager : MonoBehaviour
{
    public static SpawnLevelManager Instance;

    [Header("Scene Mode Setup")]
    [Tooltip("Check this TRUE ONLY in your Intro/Tutorial Scene. It spawns all aliens at once!")]
    [SerializeField] private bool isIntroScene = false;

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
    private bool isWaitingToSpawn = false;
    private bool isGameOver = false;

    // Track how many aliens are left alive in the intro scene
    private int tutorialAliensAlive = 0;

    // Pad structural tracking references
    private MeshRenderer padMeshRenderer;
    private Collider padCollider;
    private bool isLevelTransitioning = false;
    private bool padHasAppeared = false; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. Hide the Level Pad at the absolute start of the match
        if (levelPad != null)
        {
            padMeshRenderer = levelPad.GetComponent<MeshRenderer>();
            padCollider = levelPad.GetComponent<Collider>();
            if (padMeshRenderer != null) padMeshRenderer.enabled = false;
            if (padCollider != null) padCollider.enabled = false;
        }

        StartCoroutine(SafeInitializeGame());
    }

    private IEnumerator SafeInitializeGame()
    {
        // Wait exactly one frame for all VR systems to load securely
        yield return null; 

        // INTRO SCENE LOGIC: Force everything on immediately!
        if (isIntroScene)
        {
            tutorialAliensAlive = 0;
            foreach (GameObject alien in alienPool)
            {
                if (alien != null)
                {
                    alien.SetActive(true);
                    tutorialAliensAlive++;
                }
            }
            Debug.Log($"Intro Scene Active: All {tutorialAliensAlive} aliens have spawned at once!");
            yield break; // Stops the rest of this function from running
        }

        // --- STANDARD GAME LOGIC (Only runs when isIntroScene is FALSE) ---
        // Truly randomize the alien order in the list
        for (int i = 0; i < alienPool.Count; i++)
        {
            GameObject temp = alienPool[i];
            int randomIndex = Random.Range(i, alienPool.Count);
            alienPool[i] = alienPool[randomIndex];
            alienPool[randomIndex] = temp;
        }

        // Turn OFF every single alien completely so they cannot move or glitch in the dark
        foreach (GameObject alien in alienPool)
        {
            if (alien != null) alien.SetActive(false);
        }

        // Reveal only the very first alien to start the wave
        RevealNextAlien();
    }

    // Called automatically by your AlienHealth script when a laser strikes an alien
    public void ReportAlienDeath(GameObject destroyedAlien)
    {
        if (isGameOver) return;

        // INTRO SCENE DEATH TRACKING
        if (isIntroScene)
        {
            tutorialAliensAlive--;
            Debug.Log($"Intro alien destroyed. {tutorialAliensAlive} remaining.");
            
            if (tutorialAliensAlive <= 0)
            {
                isGameOver = true;
                StartCoroutine(RevealLevelPad());
            }
            return; // Exit out early since we don't need regular spawning logic
        }

        // --- STANDARD LEVEL DEATH TRACKING ---
        if (isWaitingToSpawn) return;

        if (destroyedAlien == currentActiveAlien || currentActiveAlien == null)
        {
            currentAlienIndex++;

            if (currentAlienIndex < alienPool.Count)
            {
                StartCoroutine(WaitAndSpawnNext());
            }
            else
            {
                isGameOver = true;
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
                currentActiveAlien.SetActive(true);
                Debug.Log($"Alien {currentAlienIndex + 1} of {alienPool.Count} is active!");
            }
        }
    }

    private IEnumerator WaitAndSpawnNext()
    {
        isWaitingToSpawn = true;
        currentActiveAlien = null; 
        yield return new WaitForSeconds(delayBetweenAliens);
        isWaitingToSpawn = false;
        RevealNextAlien();
    }

    // Delays and reveals the physical portal pad
    private IEnumerator RevealLevelPad()
    {
        padHasAppeared = true;
        yield return new WaitForSeconds(delayBeforePadAppears);
        
        if (padMeshRenderer != null) padMeshRenderer.enabled = true;
        if (padCollider != null) padCollider.enabled = true;
        Debug.Log("Victory! The Level Pad is now visible and active!");
    }

    // Detects when the VR player steps onto the teleport pad
    private void OnTriggerEnter(Collider other)
    {
        if (padHasAppeared && !isLevelTransitioning && other.CompareTag(padTag))
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    // Transitions to the next scene in Build Settings
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
