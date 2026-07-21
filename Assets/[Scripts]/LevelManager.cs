using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private string alienTag = "Alien";
    [SerializeField] private float delayBeforePadAppears = 1.5f;

    [Header("Level Pad Setup")]
    [SerializeField] private GameObject levelPad; // Drag your pad GameObject here
    [SerializeField] private string playerTag = "Player"; // Ensure your VR Rig has this tag

    private bool isPadTriggered = false;
    private bool padHasAppeared = false;

    void Start()
    {
        // Force the pad to be invisible/inactive at the start of the level
        if (levelPad != null)
        {
            levelPad.SetActive(false);
        }
    }

    void Update()
    {
        // If the pad is already visible, we don't need to check for aliens anymore
        if (padHasAppeared) return;

        // Check if any aliens are left
        GameObject[] remainingAliens = GameObject.FindGameObjectsWithTag(alienTag);

        if (remainingAliens.Length == 0)
        {
            StartCoroutine(RevealLevelPad());
        }
    }

    private IEnumerator RevealLevelPad()
    {
        padHasAppeared = true;

        // Pause briefly after the final kill before showing the pad
        yield return new WaitForSeconds(delayBeforePadAppears);

        if (levelPad != null)
        {
            levelPad.SetActive(true);
            // Optional: Play a sound effect or spawn particles here to alert the player
        }
    }

    // This detects when the VR player steps onto the pad's trigger area
    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if the pad is visible and it was the player who walked onto it
        if (padHasAppeared && !isPadTriggered && other.CompareTag(playerTag))
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadNextLevel()
    {
        isPadTriggered = true;

        // Optional: Trigger a VR fade-to-black screen effect here
        yield return new WaitForSeconds(0.5f); 

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Game Over! No more scenes left in Build Settings.");
        }
    }
}