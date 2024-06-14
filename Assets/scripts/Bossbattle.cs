using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class BossBattle : MonoBehaviour
{
    private Queue<GameObject> turnOrder = new Queue<GameObject>();
    public GameObject Player_prefab;
    public GameObject BBEG_prefab;
    public TextMeshProUGUI turnIndicatorText;
    public Button playerAttackButton;
    private PlayerController playerController;
    private MovementIndicatorManager movementIndicatorManager;
    private GameManager gameManager;
    private Transform playerTransform;
    public Button endTurnButton;
    private GameObject BBEG;
    public bool WON = false;
    public Button superAttack;
    private bool superAttackUsed = false;

    void Start()
    {
        StartCoroutine(StartDelayed());
        WON = false;
    }

    IEnumerator StartDelayed()
    {
        gameManager = FindObjectOfType<GameManager>();
        while (gameManager == null)
        {
            yield return null;
            gameManager = FindObjectOfType<GameManager>();
        }

        // Continuously search for the player GameObject until it's found
        GameObject playerGameObject = null;
        while (playerGameObject == null)
        {
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            yield return null;

            // If in battle and player not found, instantiate the player
            if (gameManager.Battle && playerGameObject == null)
            {
                gameManager.InstantiatePlayer();
            }
        }

        // Once the player GameObject is found, proceed with the initialization
        Vector3 playerSpawnPosition = GetRandomPlayerPosition();
        Vector3 BBEGSpawnPosition = GetRandomBBEGPosition();

        // Instantiate the BBEG
        BBEG = Instantiate(BBEG_prefab, BBEGSpawnPosition, Quaternion.identity);

        // Set player's position
        playerTransform = playerGameObject.transform;
        playerTransform.position = playerSpawnPosition;

        // Find and set the movement indicator manager
        movementIndicatorManager = FindObjectOfType<MovementIndicatorManager>();

        // Get the PlayerController component from the player GameObject
        playerController = playerGameObject.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController not found on the player GameObject.");
            yield break;
        }

        // Initialize movement points if the movement indicator manager is available
        if (movementIndicatorManager != null)
        {
            movementIndicatorManager.currentMovementPoints = playerController.initialMovementPoints;
        }
        else
        {
            Debug.LogError("MovementIndicatorManager not found in the scene.");
        }

        // Start combat
        StartCombat();
    }


    public void StartCombat()
    {
        Debug.Log("Starting combat...");

        // Initialize turn order with player and BBEG
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            turnOrder.Enqueue(playerGameObject);
        }
        else
        {
            Debug.LogError("Player GameObject not found when starting combat.");
        }

        if (BBEG != null)
        {
            turnOrder.Enqueue(BBEG);
        }
        else
        {
            Debug.LogError("BBEG GameObject not found when starting combat.");
        }

        // Start the first turn
        if (turnOrder.Count > 0)
        {
            NextTurn();
        }
        else
        {
            Debug.LogError("No characters in turn order queue!");
        }
    }

    public void NextTurn()
    {
        if (turnOrder.Count == 0)
        {
            Debug.LogError("Turn order queue is empty!");
            return;
        }

        var currentCharacter = turnOrder.Dequeue();

        // Check if the BBEG has been defeated before proceeding to the next turn
        if (!GameObject.FindGameObjectsWithTag("BBEG").Any())
        {
            EndBattleWithVictory();
            return; // Prevent next turn if BBEG is defeated
        }
        else if (currentCharacter == BBEG)
        {
            if (GameObject.FindGameObjectWithTag("BBEG") != null)
            {
                StartCoroutine(EnemyTurn(currentCharacter));
            }
        }
        else if (currentCharacter == playerController.gameObject)
        {
            StartCoroutine(PlayerTurn());
        }
        else
        {
            Debug.LogError("Unknown character in turn order queue or character is null!");
            NextTurn();
        }
    }


    private IEnumerator PlayerTurn()
    {
        Debug.Log("Player's turn");
        UpdateTurnIndicator("Player's Turn");
        playerController.canMove = true;
        playerAttackButton.gameObject.SetActive(true);
        endTurnButton.gameObject.SetActive(true);

        if (!superAttackUsed)
        {
            superAttack.gameObject.SetActive(true);
        }

        movementIndicatorManager.currentMovementPoints = playerController.initialMovementPoints;

        while (movementIndicatorManager.currentMovementPoints > 0)
        {
            yield return null;
        }

        Debug.Log("Player's turn ended.");

        playerAttackButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        superAttack.gameObject.SetActive(false);

        if (playerController != null)
        {
            turnOrder.Enqueue(playerController.gameObject);
            playerController.canMove = false;
            NextTurn();
        }
    }

    private IEnumerator EnemyTurn(GameObject enemyObject)
    {
        Debug.Log(enemyObject.name + ": Enemy's turn");
        UpdateTurnIndicator("Enemy's Turn");

        playerAttackButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        superAttack.gameObject.SetActive(false);

        if (enemyObject == null)
        {
            Debug.LogError("Enemy object is null.");
            NextTurn();
            yield break;
        }

        BBEG boss = enemyObject.GetComponent<BBEG>();
        if (boss == null)
        {
            Debug.LogError("BBEG component not found on enemy object.");
            NextTurn();
            yield break;
        }

        boss.ResetAttackFlag();
        boss.currentEnemyMovementPoints = boss.enemyMovementPoints;

        yield return new WaitForSeconds(0.50f);

        while (boss.currentEnemyMovementPoints > 0)
        {
            if (enemyObject == null)
            {
                Debug.LogError("Enemy object is null during actions.");
                break;
            }

            float distanceToPlayer = Vector3.Distance(boss.transform.position, playerController.transform.position);

            if (distanceToPlayer <= boss.enemyAttackRange)
            {
                boss.Attack(playerController);
                yield return new WaitForSeconds(1);
                break;
            }
            else
            {
                Vector3 directionToPlayer = (playerController.transform.position - boss.transform.position).normalized;
                Vector3 movementDirection = Vector3.zero;
                if (boss.currentEnemyMovementPoints > 0)
                {
                    int x = Mathf.RoundToInt(directionToPlayer.x);
                    int y = Mathf.RoundToInt(directionToPlayer.y);

                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        movementDirection = new Vector3(Mathf.Sign(x), 0);
                    }
                    else
                    {
                        movementDirection = new Vector3(0, Mathf.Sign(y));
                    }

                    Vector3 newPosition = boss.transform.position + movementDirection;
                    boss.Move(newPosition);

                    boss.currentEnemyMovementPoints--;
                }
            }

            yield return new WaitForSeconds(1);
        }

        if (enemyObject != null)
        {
            turnOrder.Enqueue(enemyObject);
        }

        NextTurn();
    }

    public void OnPlayerAttackButtonPressed()
    {
        if (playerController != null)
        {
            playerController.Attack();
            movementIndicatorManager.currentMovementPoints = Mathf.Max(0, movementIndicatorManager.currentMovementPoints - 2);
            StartCoroutine(EndPlayerTurnAfterAttack());
        }
    }

    public void OnSuperAttackButtonPressed()
    {
        if (playerController != null && !superAttackUsed)
        {
            playerController.SuperAttack();
            superAttack.gameObject.SetActive(false);
            superAttackUsed = true;
            StartCoroutine(EndPlayerTurnAfterAttack());
        }
    }

    public void EndPlayersTurn()
    {
        movementIndicatorManager.currentMovementPoints = 0;
        StartCoroutine(EndPlayerTurnAfterAttack());
    }

    private IEnumerator EndPlayerTurnAfterAttack()
    {
        yield return new WaitForSeconds(1);

        if (playerController != null)
        {
            movementIndicatorManager.currentMovementPoints = Mathf.Max(0, movementIndicatorManager.currentMovementPoints - 2);
            turnOrder.Enqueue(playerController.gameObject);
            NextTurn();
        }
    }

    private void UpdateTurnIndicator(string text)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = text;
        }
    }

    private Vector3 GetRandomPlayerPosition()
    {
        float randomX = Random.Range(-16, 17);
        float randomY = Random.Range(-13, 0);
        return new Vector3(randomX, randomY, -1);
    }

    private Vector3 GetRandomBBEGPosition()
    {
        float randomX = Random.Range(-17, 17);
        float randomY = Random.Range(4, 20);
        return new Vector3(randomX, randomY, -1);
    }

    public void RemoveEnemyFromQueue(GameObject enemyObject)
    {
        if (turnOrder.Contains(enemyObject))
        {
            turnOrder = new Queue<GameObject>(turnOrder.Where(character => character != enemyObject));

            if (!GameObject.FindGameObjectsWithTag("BBEG").Any())
            {
                EndBattleWithVictory();
            }
        }
    }

    private void EndBattleWithVictory()
    {
        Debug.Log("Victory! No enemies left.");
        WON = true;
        DestroyPlayerBeforeSceneSwitch();
    }

    private void DestroyPlayerBeforeSceneSwitch()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 1)
        {
            Destroy(players[0]);
        }
        else if (players.Length == 1)
        {
            Destroy(players[0]);
        }
        else
        {
            Debug.LogWarning("No player object found.");
        }

        StartCoroutine(LoadOverworldSceneAsync());
    }

    private IEnumerator LoadOverworldSceneAsync()
    {
        gameManager.levelUP();
        yield return SceneManager.LoadSceneAsync("YouWon");
    }

    private void killer(BBEG bbeg)
    {
        if (bbeg == null)
        {
            return; // Exit the method if the BBEG object is null
        }

        if (bbeg.currentEnemyHP <= 0)
        {
            RemoveEnemyFromQueue(bbeg.gameObject);
            Destroy(bbeg.gameObject);

            if (!GameObject.FindGameObjectsWithTag("BBEG").Any())
            {
                EndBattleWithVictory();
            }
        }
    }
}
