using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes via the pad
using TMPro; 

public class GameAndLevelManager : MonoBehaviour
{
    public static GameAndLevelManager Instance;

    [Header("Scene Mode Setup")]
    [Tooltip("Check this TRUE ONLY in your Tutorial Scene. It freezes the timer completely!")]
    [SerializeField] private bool isTutorialMode = false;

    [Header("Alien Management")]
    [Tooltip("Drag all the aliens placed in your scene into this list!")]
    [SerializeField] private List<GameObject> alienPool = new List<GameObject>();
    [SerializeField] private float delayBetweenAliens = 1.0f;

    [Header("Timer Settings")]
    [Tooltip("How long the player has to beat the level in the main game scene.")]
    [SerializeField] public float timeRemaining = 120.0f; 
    
    [Header("Level Pad Setup")]
    [SerializeField] private GameObject levelPad;
    [SerializeField] private string padTag = "LevelPad";
    [SerializeField] private float delayBeforePadAppears = 1.5f;

    [Header("End Game UI Setup")]
    [Tooltip("Drag the HUD Canvas that is attached to your VR Main Camera here.")]
    [SerializeField] private GameObject endGameCanvas; 
    [Tooltip("Drag the TextMeshPro text component inside that Canvas here.")]
    [SerializeField] private TextMeshProUGUI statusText; 

    private GameObject currentActiveAlien;
    private int currentAlienIndex = 0;
    private bool isWaitingToSpawn = false;
    private bool isGameOver = false;

    // Pad references
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
        // RESTORED: Hide the Level Pad at the absolute start
        if (levelPad != null)
        {
            padMeshRenderer = levelPad.GetComponent<MeshRenderer>();
            padCollider = levelPad.GetComponent<Collider>();
            if (padMeshRenderer != null) padMeshRenderer.enabled = false;
            if (padCollider != null) padCollider.enabled = false;
        }

        if (endGameCanvas != null) endGameCanvas.SetActive(false);

        StartCoroutine(SafeInitializeGame());
    }

    void Update()
    {
        if (isGameOver) return;
        if (isTutorialMode) return; 

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
            EndGame(false); // Player loses
        }
    }

    private IEnumerator SafeInitializeGame()
    {
        yield return null; 

        for (int i = 0; i < alienPool.Count; i++)
        {
            GameObject temp = alienPool[i];
            int randomIndex = Random.Range(i, alienPool.Count);
            alienPool[i] = alienPool[randomIndex];
            alienPool[randomIndex] = temp;
        }

        foreach (GameObject alien in alienPool)
        {
            if (alien != null) SetAlienVisibility(alien, false);
        }

        RevealNextAlien();
    }

    public void ReportAlienDeath(GameObject destroyedAlien)
    {
        if (isGameOver || isWaitingToSpawn) return;

        if (destroyedAlien == currentActiveAlien || currentActiveAlien == null)
        {
            currentAlienIndex++;

            if (currentAlienIndex < alienPool.Count)
            {
                StartCoroutine(WaitAndSpawnNext());
            }
            else
            {
                EndGame(true); // Player wins!
            }
        }
    }

    private void EndGame(bool playerWon)
    {
        isGameOver = true;

        if (!playerWon && currentActiveAlien != null)
        {
            Destroy(currentActiveAlien);
        }

        if (endGameCanvas != null && statusText != null)
        {
            endGameCanvas.SetActive(true);

            if (playerWon)
            {
                statusText.text = "YOU WIN!";
                statusText.color = Color.green;
                
                // RESTORED: Trigger the teleport pad to appear since they won!
                StartCoroutine(RevealLevelPad());
            }
            else
            {
                statusText.text = "GAME OVER!\nYOU LOSE";
                statusText.color = Color.red;
            }
        }
    }

    // RESTORED: Delays and reveals the physical portal pad
    private IEnumerator RevealLevelPad()
    {
        padHasAppeared = true;
        yield return new WaitForSeconds(delayBeforePadAppears);
        
        if (padMeshRenderer != null) padMeshRenderer.enabled = true;
        if (padCollider != null) padCollider.enabled = true;
        Debug.Log("Victory! The Level Pad is now visible and active!");
    }

    // RESTORED: Detects when the VR player steps onto the teleport pad
    private void OnTriggerEnter(Collider other)
    {
        if (padHasAppeared && !isLevelTransitioning && other.CompareTag(padTag))
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    // RESTORED: Transitions to the next build scene
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

    private void RevealNextAlien()
    {
        if (currentAlienIndex < alienPool.Count)
        {
            currentActiveAlien = alienPool[currentAlienIndex];
            if (currentActiveAlien != null)
            {
                SetAlienVisibility(currentActiveAlien, true);
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

    private void SetAlienVisibility(GameObject alien, bool visible)
    {
        Renderer r = alien.GetComponent<Renderer>();
        if (r != null) r.enabled = visible;

        Renderer[] childRenderers = alien.GetComponentsInChildren<Renderer>();
        foreach (Renderer cr in childRenderers) cr.enabled = visible;
    }
}
