using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab1; // First enemy prefab
    public GameObject enemyPrefab2; // Second enemy prefab
    public GameObject BossPrefab;
    public Image hpBarImage;
    public bool Battle = false;
    public static int battleAmount = 0;
    private PlayerController playerController;
    public TextMeshProUGUI levelText;
    public bool End_game;
    public AudioClip music_1; // Regular background music
    public AudioClip music_2; // Battle music
    private AudioSource audioSource;

    void Start()
    {
        DestroyEnemiesBasedOnBattleAmount();
        audioSource = GetComponent<AudioSource>();
        // Instantiate player and enemies if not in battle
        if (!Battle)
        {
            InstantiatePlayer();

        }
        else if (Battle == true)
        {
            InstantiatePlayer();
            InstantiateEnemies();
        }
        else if (End_game == true)
        {
            InstantiatePlayer();
            InstantiateBoss();
            
        }
        // Find and assign PlayerController component
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in the scene.");
            return;
        }
        // Level up after initialization
        UpdateLevelText();
        PlayBackgroundMusic();
    }

    void PlayBackgroundMusic()
    {
        if (audioSource == null) return;

        if (Battle || End_game)
        {
            audioSource.clip = music_2;
        }
        else
        {
            audioSource.clip = music_1;
        }

        audioSource.Play();
    }

    // Function to increment the battle amount
    public static void IncrementBattleAmount()
    {
        battleAmount++;
    }

    public void levelUP()
    {
        if (!Battle)
        {
            IncrementBattleAmount();
        }
        // Check if battleAmount is higher than 1
        if (battleAmount > 1)
        {
            // If PlayerController reference is not null
            if (playerController != null)
            {
                // Increase attack damage by 5 for each battle amount beyond 1
                PlayerController.attackDamage += (battleAmount - 1) + 5f;
                Debug.Log("Attack Damage increased to: " + PlayerController.attackDamage);
            }
            else
            {
                Debug.LogWarning("PlayerController not found in the scene.");
            }
        }
        if (battleAmount == 1)
        {

        }
    }

    public void Fight()
    {
        Battle = true;
        IncrementBattleAmount();
        SceneSwitcher sceneSwitcher = FindObjectOfType<SceneSwitcher>();
        if (sceneSwitcher != null)
        {
            sceneSwitcher.LoadScene("battleArena");
        }
    }

    public void BossFight()
    {
        End_game = true;
        IncrementBattleAmount();
        InstantiateBoss(); // Instantiate the BBEG
        SceneSwitcher sceneSwitcher = FindObjectOfType<SceneSwitcher>();
        if (sceneSwitcher != null)
        {
            sceneSwitcher.LoadScene("Bossbattle");
        }
    }

    public void InstantiateBoss()
    {
        Vector3 bossPosition = RandomEnemyPosition(); // Modify this to set the desired position for the BBEG
        Instantiate(BossPrefab, bossPosition, Quaternion.identity);
    }

    public void InstantiatePlayer()
    {
        Vector3 startPosition = Battle ? RandomPlayerStartPosition() : Vector3.zero;
        Instantiate(playerPrefab, startPosition, Quaternion.identity);
    }

    private void InstantiateEnemies()
    {
        int enemyAmount = Random.Range(1, 5);
        for (int i = 0; i < enemyAmount; i++)
        {
            Vector3 enemyPosition = RandomEnemyPosition();

            // Randomly select between enemyPrefab1 and enemyPrefab2
            GameObject selectedEnemyPrefab = Random.Range(0, 2) == 0 ? enemyPrefab1 : enemyPrefab2;

            GameObject enemyInstance = Instantiate(selectedEnemyPrefab, enemyPosition, Quaternion.identity);
        }
    }

    private Vector3 RandomPlayerStartPosition()
    {
        float randomX = Random.Range(-16, 17);
        float randomY = Random.Range(-13, 0);
        return new Vector3(randomX, randomY, -1);
    }

    private Vector3 RandomEnemyPosition()
    {
        float randomX = Random.Range(-17, 17);
        float randomY = Random.Range(4, 20);
        return new Vector3(randomX, randomY, -1);
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = ("level " + battleAmount.ToString());
        }
        else
        {
            Debug.LogWarning("Level text reference is not set in the GameManager.");
        }
    }
    public void DestroyEnemiesBasedOnBattleAmount()
    {
        switch (battleAmount)
        {
            case 1:
                Destroy(GameObject.Find("enemy"));
                break;
            case 2:
                Destroy(GameObject.Find("enemy"));
                Destroy(GameObject.Find("enemy (1)"));
                break;
            case 3:
                Destroy(GameObject.Find("enemy"));
                Destroy(GameObject.Find("enemy (1)"));
                Destroy(GameObject.Find("enemy (2)"));
                break;
            case 4:
                Destroy(GameObject.Find("enemy"));
                Destroy(GameObject.Find("enemy (1)"));
                Destroy(GameObject.Find("enemy (2)"));
                Destroy(GameObject.Find("enemy (3)"));
                break;
            case 5:
                Destroy(GameObject.Find("enemy"));
                Destroy(GameObject.Find("enemy (1)"));
                Destroy(GameObject.Find("enemy (2)"));
                Destroy(GameObject.Find("enemy (3)"));
                Destroy(GameObject.Find("enemy (4)"));
                break;
            case 6:
                Destroy(GameObject.Find("enemy"));
                Destroy(GameObject.Find("enemy (1)"));
                Destroy(GameObject.Find("enemy (2)"));
                Destroy(GameObject.Find("enemy (3)"));
                Destroy(GameObject.Find("enemy (4)"));
                Destroy(GameObject.Find("enemy (5)"));
                break;
            default:
                Debug.LogWarning("No action defined for battle amount: " + battleAmount);
                break;
        }
    }



}
