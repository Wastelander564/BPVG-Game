using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Konami : MonoBehaviour
{
    public AudioClip konamiAudioClip; // Audio clip to play upon entering the KonamiScene
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource component if not already present
        }

        PlayKonamiAudio();
    }

    void PlayKonamiAudio()
    {
        // Play audio clip if available
        if (konamiAudioClip != null)
        {
            audioSource.PlayOneShot(konamiAudioClip);
        }

        // Start coroutine to return to the Overworld scene after a delay
        StartCoroutine(ReturnToOverworldAfterDelay());
    }

    IEnumerator ReturnToOverworldAfterDelay()
    {
        // Wait for the audio clip duration + additional delay
        yield return new WaitForSeconds(konamiAudioClip.length);

        // Return to the Overworld scene
        SceneManager.LoadScene("Overworld");
    }
}