using UnityEngine;
using TMPro; // Required if using TextMeshPro UI

public class TimerDisplay : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 60f; // Initial countdown duration in seconds
    public bool timerIsActive = true;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // Drag your TextMeshPro object here

    void Update()
    {
        if (timerIsActive)
        {
            if (timeRemaining > 0)
            {
                // Subtract the time passed since the last frame
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsActive = false;
                DisplayTime(timeRemaining);
                
                // Add game over or trigger events here
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        // Prevent negative values from displaying
        if (timeToDisplay < 0) timeToDisplay = 0; 

        // Calculate minutes and seconds
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // Formats string to display as 00:00 (two-digit formatting)
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}