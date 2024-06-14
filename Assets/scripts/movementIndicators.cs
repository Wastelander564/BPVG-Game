using UnityEngine;
using System.Collections;

public class MovementIndicator : MonoBehaviour
{
    private Transform playerTransform;

    void Start()
    {
        StartCoroutine(StartDelayed());
        // Get reference to the player's transform
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Handle click event when the movement indicator is clicked
    void OnMouseDown()
    {
        // Get the clicked position
        Vector3 clickedPosition = transform.position;

        // Move the player to the clicked position
        playerTransform.position = clickedPosition;

        // Update movement indicators after player movement
        MovementIndicatorManager manager = GetComponentInParent<MovementIndicatorManager>();
        if (manager != null)
        {
            manager.UpdateMovementIndicators();
        }
    }
    IEnumerator StartDelayed()
    {
        yield return new WaitForSeconds(5);

    }
}