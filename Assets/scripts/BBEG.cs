using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BBEG : MonoBehaviour
{
    public bool final = false;
    public float enemyHP = 50.0f;
    public float currentEnemyHP;
    public int enemyMovementPoints = 5;
    public int enemyAttackRange = 1;
    public float damageAmount = 20f; // Damage dealt by the enemy
    public int currentEnemyMovementPoints;
    private Transform player; // Reference to the player
    private PlayerController playerController; // Reference to the player controller
    private GameManager gameManager;
    private bool hasAttacked = false; // Flag to track if the enemy has attacked in its turn
    private CombatManager combatManager;
    private Animator animator;
    public GameObject particleEffectPrefab;

    void Start()
    {
        // Initialize references
        StartCoroutine(InitializeReferences());
    }

    private IEnumerator InitializeReferences()
    {
        // Find the GameManager
        gameManager = FindObjectOfType<GameManager>();

        // Wait until the player is available
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            yield return null;
        }

        // Get the PlayerController component
        playerController = player.GetComponent<PlayerController>();

        // Wait until the player is fully initialized
        while (playerController == null || !playerController.IsInitialized)
        {
            yield return null;
        }

        currentEnemyHP = enemyHP;
        currentEnemyMovementPoints = enemyMovementPoints;
        combatManager = FindObjectOfType<CombatManager>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && gameManager != null)
        {
            if (!gameManager.Battle && !gameManager.End_game)
            {
                final = true;
                gameManager.BossFight();
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentEnemyHP -= damageAmount;
        if (currentEnemyHP <= 0)
        {
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void Attack(PlayerController player)
    {
        if (player != null && !hasAttacked)
        {
            // Trigger attack animation
            animator.SetBool("Attack", true);

            // Start coroutine to handle attack after animation
            StartCoroutine(HandleAttack(player));
        }
    }

    private IEnumerator HandleAttack(PlayerController player)
    {
        // Wait until the end of the attack animation
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Ensure player still exists and hasn't been destroyed during the animation
        if (player != null)
        {
            player.TakeDamage(damageAmount);
            hasAttacked = true;

            // Set animator back to idle
            animator.SetBool("Attack", false);
        }
    }

    public void Move(Vector3 newPosition)
    {
        // Check if the new position is valid
        if (IsValidMove(newPosition))
        {
            transform.position = newPosition;
            animator.SetBool("IsMoving", true);

            if (transform.position == newPosition)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    private bool IsValidMove(Vector3 newPosition)
    {
        // Perform collision detection or boundary checks here
        // For simplicity, let's assume movement is always valid
        return true;
    }

    // Method to reset the attack flag for the enemy
    public void ResetAttackFlag()
    {
        hasAttacked = false;
    }

    public void DestroyEnemy()
    {
        // Ensure the CombatManager reference is not null
        // Destroy the enemy gameObject
        Destroy(gameObject);
        Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        SceneManager.LoadScene("YouWon");
    }
}
