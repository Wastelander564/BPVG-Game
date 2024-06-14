using System.Collections;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private GameObject player; // Reference to the player object
    private Vector3 offset = new Vector3(0, 0, -10);
    private GameManager gameManager;
    private PlayerController playerController; // Reference to the PlayerController component

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // Start the coroutine to find and wait for the player object
        StartCoroutine(FindAndAwaitPlayerObject());
    }

    void LateUpdate()
    {
        // Check if the player object reference is valid
        if (player != null)
        {
            transform.position = player.transform.position + offset;
        }
    }

    private IEnumerator FindAndAwaitPlayerObject()
    {
        while (player == null)
        {
            // Try to find the player object by tag
            player = GameObject.FindGameObjectWithTag("Player");

            // If the player object is still not found, wait for the next frame
            if (player == null)
            {
                Debug.LogWarning("Player object not found! Retrying...");
                yield return null;
            }
        }

        // Get the PlayerController component
        playerController = player.GetComponent<PlayerController>();

        // Wait until the player is fully initialized
        while (playerController == null || !playerController.IsInitialized)
        {
            Debug.LogWarning("Player object found, but not initialized. Waiting...");
            yield return null;
        }

        Debug.Log("Player object found and initialized.");
    }
}