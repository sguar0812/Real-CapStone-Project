using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private string alienTag = "Alien";
    [SerializeField] private float delayBeforePadAppears = 1.5f;

    [Header("Level Pad Setup")]
    [SerializeField] private GameObject levelPad; // Drag your pad object here!
    [SerializeField] private string padTag = "LevelPad"; // Give your pad this tag

    private MeshRenderer padMeshRenderer;
    private Collider padCollider;
    private bool isLevelTransitioning = false;
    private bool padHasAppeared = false;

    void Start()
    {
        // Safely find the pad's visual and physical components
        if (levelPad != null)
        {
            padMeshRenderer = levelPad.GetComponent<MeshRenderer>();
            padCollider = levelPad.GetComponent<Collider>();

            // Hide it immediately at the start of the level
            if (padMeshRenderer != null) padMeshRenderer.enabled = false;
            if (padCollider != null) padCollider.enabled = false;
        }
    }

    void Update()
    {
        if (padHasAppeared) return;

        GameObject[] remainingAliens = GameObject.FindGameObjectsWithTag(alienTag);

        if (remainingAliens.Length == 0)
        {
            StartCoroutine(RevealLevelPad());
        }
    }

    private IEnumerator RevealLevelPad()
    {
        padHasAppeared = true;
        yield return new WaitForSeconds(delayBeforePadAppears);

        if (padMeshRenderer != null) padMeshRenderer.enabled = true;
        if (padCollider != null) padCollider.enabled = true;
        
        Debug.Log("The Player script has successfully revealed the Level Pad!");
    }

    // Now running on the player, this detects when YOU step on the pad
    private void OnTriggerEnter(Collider other)
    {
        // If the pad is active, we haven't loaded yet, and we just stepped on the LevelPad
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
        else
        {
            Debug.Log("Game Over! No more scenes left in Build Settings.");
        }
    }
}