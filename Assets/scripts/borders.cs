using UnityEngine;

public class Border : MonoBehaviour
{
    // Tag for the border objects
    public string borderTag = "Border";

    // Start position of the player
    private Vector3 startPosition = new Vector3(0, 0, 0);

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has the border tag
        if (collision.gameObject.CompareTag(borderTag))
        {
            // Reset the player's position to the start position
            transform.position = startPosition;
        }
    }
}