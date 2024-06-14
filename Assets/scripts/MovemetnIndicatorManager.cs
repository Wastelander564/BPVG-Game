using UnityEngine;
using System.Collections;

public class MovementIndicatorManager : MonoBehaviour
{
    public GameObject movementIndicatorPrefab;
    public int currentMovementPoints; // Current movement points

    private Transform PlayerPosition;
    private Enemy enemy; // Reference to the enemy
    private GameManager gameManager;
    private PlayerController playerController;
    public int CurrentMovementPoints => currentMovementPoints;

    void Start()
    {
        StartCoroutine(StartDelayed());
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            // Get reference to the player's transform
            PlayerPosition = playerGameObject.transform;

            // Find the PlayerController component in the scene
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found in the scene.");
                return;
            }

            // Get reference to the enemy instance
            enemy = FindObjectOfType<Enemy>();
            gameManager = FindObjectOfType<GameManager>();

            // Only update movement indicators if battle is true
            if (gameManager != null && (gameManager.Battle || gameManager.End_game))
            {
                UpdateMovementIndicators();
            }

            currentMovementPoints = playerController.initialMovementPoints; // Initialize current movement points
        }
        else
        {
            Debug.LogError("Player GameObject not found in the scene.");
        }
    }

    void Update()
    {
        // Check if PlayerTransform is not null before accessing its position
        if (PlayerPosition != null)
        {
            // Follow the player by setting the manager's position to match the player's position
            transform.position = PlayerPosition.position;
        }
    }

    public void UpdateMovementIndicators()
    {
        if (gameManager == null || (!gameManager.Battle && !gameManager.End_game))
        {
            return;
        }

        // Remove existing movement indicator blocks
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Calculate valid movement positions around the player
        Vector3[] validPositions = CalculateValidMovementPositions();

        // Spawn movement indicator blocks at valid positions
        foreach (Vector3 position in validPositions)
        {
            GameObject indicator = Instantiate(movementIndicatorPrefab, position, Quaternion.identity, transform);
            // Set the z-index to -1 to ensure it appears on the grid
            indicator.transform.position = new Vector3(indicator.transform.position.x, indicator.transform.position.y, -1);
            // Attach MovementIndicator script to each indicator
            indicator.AddComponent<MovementIndicator>();
        }
    }

    Vector3[] CalculateValidMovementPositions()
    {
        // Calculate valid movement positions based on movement points
        // For a 1x1 grid, the valid positions are within a diamond shape around the player

        int gridSize = 2 * currentMovementPoints + 1; // Size of the grid
        Vector3[] validPositions = new Vector3[gridSize * gridSize - 1]; // Maximum number of valid positions excluding the center

        // Calculate valid positions relative to the player's current position
        Vector3 playerPosition = PlayerPosition.position; // Assuming the manager is attached to the player GameObject

        // Round player's position to nearest integer values
        int roundedX = Mathf.RoundToInt(playerPosition.x);
        int roundedY = Mathf.RoundToInt(playerPosition.y);

        // Calculate player's center position
        Vector3 playerCenter = new Vector3(roundedX, roundedY, playerPosition.z);

        // Add adjacent positions within the diamond shape
        int index = 0;
        for (int x = -currentMovementPoints; x <= currentMovementPoints; x++)
        {
            for (int y = -currentMovementPoints; y <= currentMovementPoints; y++)
            {
                // Skip positions outside the diamond shape based on movement points
                if (Mathf.Abs(x) + Mathf.Abs(y) > currentMovementPoints)
                    continue;

                // Skip the center position (where the player is located)
                if (x == 0 && y == 0)
                    continue;

                // Calculate the position relative to the player's center
                Vector3 position = playerCenter + new Vector3(x, y, 0);
                validPositions[index] = position;
                index++;
            }
        }

        return validPositions;
    }

    // Function to decrease movement points
    public void DecreaseMovementPoints()
    {
        if (gameManager != null && (gameManager.Battle || gameManager.End_game))
        {
            currentMovementPoints = Mathf.Max(0, currentMovementPoints - 1);
            UpdateMovementIndicators();
        }
    }

    IEnumerator StartDelayed()
    {
        yield return new WaitForSeconds(3); // Delay initialization

        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            PlayerPosition = playerGameObject.transform;
            playerController = playerGameObject.GetComponent<PlayerController>();

            if (playerController == null)
            {
                Debug.LogError("PlayerController not found on the player GameObject.");
                yield break;
            }

            currentMovementPoints = playerController.initialMovementPoints;

            // Only update movement indicators if battle is true
            if (gameManager != null && (gameManager.Battle || gameManager.End_game))
            {
                UpdateMovementIndicators();
            }
        }
        else
        {
            Debug.LogError("Player GameObject not found in the scene.");
        }
    }
}
