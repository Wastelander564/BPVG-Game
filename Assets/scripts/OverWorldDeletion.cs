using System.Collections.Generic;
using UnityEngine;

public class PlayerEnemyCollision : MonoBehaviour
{
    // Static list to store the tags of enemies the player has collided with
    private static List<string> collidedEnemyTags = new List<string>();

    // Called when the player collides with an enemy
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Remember the tag of the collided enemy
            collidedEnemyTags.Add(collision.tag);
        }
    }

    // Destroy all enemies with the remembered tags
    public static void DestroyEnemiesWithRememberedTags()
    {
        foreach (string enemyTag in collidedEnemyTags)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
        }

    }
}