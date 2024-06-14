using System.Collections;
using UnityEngine;

public class MichealHere : MonoBehaviour
{
    public GameObject childObject; // The child object to activate
    public AudioClip audioClip; // Audio clip to play
    public float delayInSeconds = 5.0f; // Delay before the child appears and audio plays

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        StartCoroutine(ActivateChildAndPlayAudio());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator ActivateChildAndPlayAudio()
    {
        yield return new WaitForSeconds(delayInSeconds);

        // Activate the child object
        if (childObject != null)
        {
            childObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Child object is not assigned.");
        }

        // Play the audio clip
        audioSource.Play();
    }
}