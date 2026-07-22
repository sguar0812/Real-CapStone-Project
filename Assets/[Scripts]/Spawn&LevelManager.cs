using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
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
        // Hide the Level Pad at the absolute start
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

        // Truly randomize the alien order in the list
        for (int i = 0; i < alienPool.Count; i++)
        {
            GameObject temp = alienPool[i];
            int randomIndex = Random.Range(i, alienPool.Count);
            alienPool[i] = alienPool[randomIndex];
            alienPool[randomIndex] = temp;
        }

        // Turn OFF every single alien completely so they cannot glitch out or move in the dark
        foreach (GameObject alien in alienPool)
        {
            if (alien != null) alien.SetActive(false);
        }

        // Turn on and reveal the very first alien to start the game
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
                StartCoroutine(RevealLevelPad());
            }
            else
            {
                statusText.text = "GAME OVER!\nYOU LOSE";
                statusText.color = Color.red;
            }
        }
    }

    // Explicitly placed here so EndGame can find it perfectly
    private IEnumerator RevealLevelPad()
    {
        padHasAppeared = true;
        yield return new WaitForSeconds(delayBeforePadAppears);
        
        if (padMeshRenderer != null) padMeshRenderer.enabled = true;
        if (padCollider != null) padCollider.enabled = true;
        Debug.Log("Victory! The Level Pad is now visible and active!");
    }

    private void RevealNextAlien()
    {
        if (currentActiveAlien != null)
        {
            currentActiveAlien.SetActive(false);
        }

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
