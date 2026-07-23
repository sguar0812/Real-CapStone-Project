using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnStart : MonoBehaviour
{
    // Allows you to drag and drop your sound file in the Inspector
    public AudioClip sceneStartSound; 
    private AudioSource audioSource;

    void Start()
    {
        // Get or add the AudioSource component on this GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Play the assigned sound clip once
        if (sceneStartSound != null)
        {
            audioSource.PlayOneShot(sceneStartSound);
        }
    }
}