using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro

public class GameAndLevelManager : MonoBehaviour
{
    // Global instance lets your laser script talk directly to this script
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
    [SerializeField] public float timeRemaining = 120.0f; // PUBLIC so your friend's timer script can read it!
    
    [Header("End Game UI Setup")]
    [Tooltip("Drag the HUD Canvas that is attached to your VR Main Camera here.")]
    [SerializeField] private GameObject endGameCanvas; 
    [Tooltip("Drag the TextMeshPro text component inside that Canvas here.")]
    [SerializeField] private TextMeshProUGUI statusText; 

    private GameObject currentActiveAlien;
    private int currentAlienIndex = 0;
    private bool isWaitingToSpawn = false;
    private bool isGameOver = false;

    void Awake()
    {
        // Set up the static reference so the Laser can find this manager instantly
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Force the game over text canvas to be invisible at the start of the match
        if (endGameCanvas != null) endGameCanvas.SetActive(false);

        StartCoroutine(SafeInitializeGame());
    }

    void Update()
    {
        if (isGameOver) return;
        
        // FIX: If this is the tutorial scene, skip the countdown entirely so the timer never runs out!
        if (isTutorialMode) return; 

        // Countdown timer tick for the main gameplay scene
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
            EndGame(false); // Timer hit zero: PLAYER LOSES
        }
    }

    private IEnumerator SafeInitializeGame()
    {
        // Wait exactly one frame for all VR systems and alien components to fully awaken
        yield return null; 

        // Truly randomize the alien order
        for (int i = 0; i < alienPool.Count; i++)
        {
            GameObject temp = alienPool[i];
            int randomIndex = Random.Range(i, alienPool.Count);
            alienPool[i] = alienPool[randomIndex];
            alienPool[randomIndex] = temp;
        }

        // Hide ALL aliens safely now that initialization is stable
        foreach (GameObject alien in alienPool)
        {
            if (alien != null) SetAlienVisibility(alien, false);
        }

        // Reveal the very first alien to start the game
        RevealNextAlien();
    }

    // Called automatically by your AlienHealth script when it takes a laser hit
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
                EndGame(true); // All aliens dead: PLAYER WINS!
            }
        }
    }

    private void EndGame(bool playerWon)
    {
        isGameOver = true;

        // Clean up remaining current alien visual if the player lost by running out of time
        if (!playerWon && currentActiveAlien != null)
        {
            Destroy(currentActiveAlien);
        }

        // Make the hidden VR head-HUD canvas appear instantly
        if (endGameCanvas != null && statusText != null)
        {
            endGameCanvas.SetActive(true);

            if (playerWon)
            {
                statusText.text = "YOU WIN!";
                statusText.color = Color.green;
                Debug.Log("Game ended: Player won!");
            }
            else
            {
                statusText.text = "GAME OVER!\nYOU LOSE";
                statusText.color = Color.red;
                Debug.Log("Game ended: Player ran out of time!");
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

        Renderer[] childRenderers = alien.GetComponentsInChildren<Renderer>();
        foreach (Renderer cr in childRenderers) cr.enabled = visible;
    }
}
