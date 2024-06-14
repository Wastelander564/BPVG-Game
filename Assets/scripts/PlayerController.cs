using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float movement;
    public HPBar hpBar;
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isMoving;
    public bool canMove = true;
    private Vector2 input;
    public MovementIndicatorManager indicatorManager;
    public Vector3 playerStartPosition;
    private Enemy enemy;
    private BBEG bbeg;
    private GameManager gameManager;
    public int initialMovementPoints = 3;
    public static float attackDamage = 10f;
    public int attackRange = 1;
    public bool collided = false;
    public bool IsInitialized { get; private set; } = false;
    private Animator animator;
    public GameObject particleEffectPrefab;
    public bool isResetting;

    private List<KeyCode> konamiCode;
    private int konamiCodeIndex;
    public GameObject scaryObject;
    private AudioSource audioSource;

    void Start()
    {
        IsInitialized = true;

        indicatorManager = FindObjectOfType<MovementIndicatorManager>();
        if (indicatorManager == null)
        {
            Debug.LogError("MovementIndicatorManager not found in the scene.");
            return;
        }

        hpBar = FindObjectOfType<HPBar>();
        if (hpBar == null)
        {
            Debug.LogError("HPBar not found in the scene.");
            return;
        }

        currentHealth = maxHealth;
        hpBar.SetHP(maxHealth);
        playerStartPosition = transform.position;

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the player GameObject.");
            return;
        }

        // Initialize the Konami Code sequence
        konamiCode = new List<KeyCode>
        {
            KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow,
            KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.B, KeyCode.A
        };
        audioSource = GetComponent<AudioSource>();
        konamiCodeIndex = 0;
    }

    void Update()
    {
        if (!isMoving && canMove)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0 || input.y != 0)
            {
                if (input.y > 0)
                {
                    animator.SetInteger("Direction", 1);
                }
                else if (input.y < 0)
                {
                    animator.SetInteger("Direction", 3);
                }
                else if (input.x > 0)
                {
                    animator.SetInteger("Direction", 4);
                }
                else if (input.x < 0)
                {
                    animator.SetInteger("Direction", 2);
                }

                var targetPos = transform.position + new Vector3(input.x, input.y, 0);
                StartCoroutine(Move(targetPos));
                indicatorManager.DecreaseMovementPoints();
                playerStartPosition = transform.position;
            }
            else
            {
                animator.SetInteger("Direction", 0);
            }
        }

        if (!isMoving)
        {
            animator.SetInteger("Direction", 0);
        }

        // Check for Konami Code input
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(konamiCode[konamiCodeIndex]))
            {
                konamiCodeIndex++;
                if (konamiCodeIndex >= konamiCode.Count)
                {
                    TriggerKonamiCodeAction();
                    konamiCodeIndex = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                     Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                     Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.A))
            {
                konamiCodeIndex = 0;
            }
        }
    }

    private void TriggerKonamiCodeAction()
    {
        // Implement the action to be triggered when the Konami Code is entered

        Debug.Log("Konami Code Entered!");
        SceneManager.LoadScene("Konami");
        // Add your custom action here
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, movement * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        hpBar.TakeDamage(damageAmount);
        if (currentHealth <= 0)
        {
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            SceneManager.LoadScene("Defeated");
        }
    }

    public void Attack()
    {
        StartCoroutine(HandleAttackAnimation("attacking", attackDamage));
    }

    public void SuperAttack()
    {
        StartCoroutine(HandleAttackAnimation("supperattack", 40f));
    }

    private IEnumerator HandleAttackAnimation(string animationBool, float damage)
    {
        animator.SetBool(animationBool, true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (enemy != null)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) <= attackRange)
            {
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.Log("Enemy is out of range.");
            }
        }
        else if (bbeg != null)
        {
            if (Vector3.Distance(transform.position, bbeg.transform.position) <= attackRange)
            {
                bbeg.TakeDamage(damage);
            }
            else
            {
                Debug.Log("BBEG is out of range.");
            }
        }
        else
        {
            Debug.Log("No enemy or BBEG in range.");
        }

        animator.SetBool(animationBool, false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemy = other.GetComponent<Enemy>();
        }
        else if (other.CompareTag("BBEG"))
        {
            bbeg = other.GetComponent<BBEG>();
        }
        else if (other.CompareTag("Border"))
        {
            SceneManager.LoadScene("OverWorld");
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemy = null;
        }
        else if (other.CompareTag("BBEG"))
        {
            bbeg = null;
        }
    }
    IEnumerator wait()
    {
        yield return new WaitForSeconds(3f);
    }
}
