using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    private List<GameObject> characters = new List<GameObject>(); // List to hold all characters
    public GameObject Player_prefab;
    public GameObject Enemy_prefab;
    public TextMeshProUGUI turnIndicatorText; // Reference to the TextMeshProUGUI component
    public Button playerAttackButton; // Reference to the UI button
    private PlayerController playerController;
    private MovementIndicatorManager movementIndicatorManager;
    private GameManager gameManager;
    private bool WON = false;
    public Button superAttack;

    // Flag to track if super attack has been used
    private bool superAttackUsed = false;

    void Start()
    {
        StartCoroutine(StartDelayed());
        WON = false;
    }

    IEnumerator StartDelayed()
    {
        // Wait for the GameManager and player to be instantiated
        gameManager = FindObjectOfType<GameManager>();
        while (gameManager == null)
        {
            yield return null; // Wait until the GameManager is available
            gameManager = FindObjectOfType<GameManager>();
        }

        // Wait for the player to be instantiated
        while (GameObject.FindGameObjectWithTag("Player") == null)
        {
            yield return null; // Wait until the player is instantiated
            if (gameManager.Battle)
            {
                gameManager.InstantiatePlayer();
            }
        }

        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            // Add player to the characters list
            characters.Add(playerGameObject);
            playerController = playerGameObject.GetComponent<PlayerController>();

            // Find the MovementIndicatorManager instance in the scene
            movementIndicatorManager = FindObjectOfType<MovementIndicatorManager>();

            // Initialize current movement points
            if (movementIndicatorManager != null)
            {
                movementIndicatorManager.currentMovementPoints = playerController.initialMovementPoints;
            }
            else
            {
                Debug.LogError("MovementIndicatorManager not found in the scene.");
            }

            // Start combat after initialization
            StartCombat();
        }
        else
        {
            Debug.LogError("Player GameObject not found in the scene.");
        }
    }

    public void StartCombat()
    {
        Debug.Log("Starting combat...");

        // Instantiate enemies and add them to the characters list
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            characters.Add(enemy);
        }

        StartCoroutine(CombatLoop());
    }

    IEnumerator CombatLoop()
    {
        while (!WON)
        {
            yield return StartCoroutine(PlayerTurn());

            // Filter the list to remove null or destroyed enemies
            characters = characters.Where(c => c != null).ToList();

            foreach (GameObject character in characters.Where(c => c.CompareTag("Enemy")).ToList())
            {
                if (character != null)
                {
                    yield return StartCoroutine(EnemyTurn(character));
                }

                // Check if victory condition is met after each enemy's turn
                if (!GameObject.FindGameObjectsWithTag("Enemy").Any())
                {
                    EndBattleWithVictory();
                    yield break;
                }
            }

            // Additional check if all enemies are defeated after enemy turns
            if (!GameObject.FindGameObjectsWithTag("Enemy").Any())
            {
                EndBattleWithVictory();
                yield break;
            }
        }
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("Player's turn");
        UpdateTurnIndicator("Player's Turn");
        playerController.canMove = true;
        // Enable the attack button
        playerAttackButton.gameObject.SetActive(true);

        // Enable the super attack button if it hasn't been used
        if (!superAttackUsed)
        {
            superAttack.gameObject.SetActive(true);
        }

        // Reset player's movement points at the start of their turn
        movementIndicatorManager.currentMovementPoints = playerController.initialMovementPoints;

        // Wait until the player has taken their turn
        while (movementIndicatorManager.currentMovementPoints > 0)
        {
            yield return null;
        }

        Debug.Log("Player's turn ended.");

        // Disable the attack button
        playerAttackButton.gameObject.SetActive(false);

        // Disable the super attack button
        superAttack.gameObject.SetActive(false);

        // End player's turn
        playerController.canMove = false;
    }

    private IEnumerator EnemyTurn(GameObject enemyObject)
    {
        Debug.Log(enemyObject.name + ": Enemy's turn");
        UpdateTurnIndicator("Enemy's Turn");

        // Disable the attack button during enemy's turn
        playerAttackButton.gameObject.SetActive(false);
        superAttack.gameObject.SetActive(false);

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemyObject == null || !enemyObject.activeSelf)
        {
            // If the enemy is destroyed, remove it from the turn order and skip its turn
            Debug.LogWarning("Enemy object is null or destroyed during its turn.");
            yield break;
        }

        if (enemy == null)
        {
            Debug.LogError("Enemy component not found on enemy object.");
            yield break;
        }

        // Reset the hasAttacked flag for the enemy
        enemy.ResetAttackFlag();
        enemy.currentEnemyMovementPoints = enemy.enemyMovementPoints;

        yield return new WaitForSeconds(0.50f);

        // Execute enemy's actions
        while (enemy.currentEnemyMovementPoints > 0)
        {
            if (enemyObject == null)
            {
                Debug.LogError("Enemy object is null during actions.");
                break;
            }

            // Calculate the distance to the player
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, playerController.transform.position);

            // If player is within attack range, attack the player
            if (distanceToPlayer <= enemy.enemyAttackRange)
            {
                enemy.Attack(playerController);

                // End enemy's turn after attacking
                yield return new WaitForSeconds(1); // Adjust timing as needed
                break;
            }
            else
            {
                // Calculate the direction towards the player
                Vector3 directionToPlayer = (playerController.transform.position - enemy.transform.position).normalized;

                // Determine the movement direction based on available movement points
                Vector3 movementDirection = Vector3.zero;
                if (enemy.currentEnemyMovementPoints > 0)
                {
                    // Round the direction to the nearest integer to ensure movement only along X or Y axis
                    int x = Mathf.RoundToInt(directionToPlayer.x);
                    int y = Mathf.RoundToInt(directionToPlayer.y);

                    // Clamp the movement direction to either left/right or up/down
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        movementDirection = new Vector3(Mathf.Sign(x), 0, 0);
                    }
                    else
                    {
                        movementDirection = new Vector3(0, Mathf.Sign(y), 0);
                    }

                    // Update the enemy's position based on the movement direction
                    Vector3 newPosition = enemy.transform.position + movementDirection;
                    enemy.Move(newPosition);

                    // Reduce movement points
                    enemy.currentEnemyMovementPoints--;
                }
            }

            yield return new WaitForSeconds(1); // Simulate enemy action delay
        }
    }

    public void OnPlayerAttackButtonPressed()
    {
        if (playerController != null)
        {
            playerController.Attack();

            // End player's turn after the attack animation or action is completed
            StartCoroutine(EndPlayerTurnAfterAttack());
        }
    }

    public void OnPlayerSuperAttackButtonPressed()
    {
        if (playerController != null && !superAttackUsed)
        {
            playerController.SuperAttack();
            superAttack.gameObject.SetActive(false);
            superAttackUsed = true; // Mark super attack as used

            // End player's turn after the attack animation or action is completed
            StartCoroutine(EndPlayerTurnAfterAttack());
        }
    }

    public void EndPlayersTurn()
    {
        // Update the remaining movement points
        movementIndicatorManager.currentMovementPoints = 0;

        // Proceed to the next turn
        StartCoroutine(EndPlayerTurnAfterAttack());
    }

    private IEnumerator EndPlayerTurnAfterAttack()
    {
        // Wait for the attack animation or action to complete
        // Replace this with appropriate wait logic based on your game's attack mechanism
        yield return new WaitForSeconds(1); // Example: Wait for 1 second as a placeholder

        // Update movement points to zero
        movementIndicatorManager.currentMovementPoints = 0;
    }

    private void UpdateTurnIndicator(string text)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = text;
        }
    }

    private void EndBattleWithVictory()
    {
        Debug.Log("Victory! No enemies left.");
        WON = true;

        // Destroy the player object before scene switching
        DestroyPlayerBeforeSceneSwitch();
    }

    private void DestroyPlayerBeforeSceneSwitch()
    {
        // Find all player objects in the scene
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // If there's more than one player object, destroy one
        if (players.Length > 1)
        {
            // Destroy one of the player objects
            Destroy(players[0]);
        }
        else if (players.Length == 1)
        {
            // Store player's position in PlayerPrefs
            GameObject player = players[0];
            Vector3 playerPosition = player.transform.position;
            PlayerPrefs.SetFloat("PlayerX", playerPosition.x);
            PlayerPrefs.SetFloat("PlayerY", playerPosition.y);
            PlayerPrefs.SetFloat("PlayerZ", playerPosition.z);

            // Don't destroy the last player object
        }
        else
        {
            Debug.LogError("No player objects found.");
        }

        // Start loading the overworld scene
        StartCoroutine(LoadOverworldSceneAsync());
    }

    private IEnumerator LoadOverworldSceneAsync()
    {
        // Load the overworld scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("OverWorld");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
