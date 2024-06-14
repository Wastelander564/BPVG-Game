using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Image hpBarImage; // Reference to the UI Image representing the HP bar
    public float maxHP = 100f; // Maximum HP
    public float currentHP; // Current HP

    void Start()
    {
        currentHP = maxHP; // Initialize current HP to max HP
        UpdateHPBar(); // Update HP bar UI
    }

    // Function to update HP
    public void SetHP(float hp)
    {
        currentHP = hp; // Update current HP
        UpdateHPBar(); // Update HP bar UI
    }

    // Function to decrease HP
    public void TakeDamage(float damageAmount)
    {
        currentHP -= damageAmount; // Reduce current HP by damageAmount
        if (currentHP < 0f)
        {
            currentHP = 0f; // Ensure HP doesn't go below 0
        }
        UpdateHPBar(); // Update HP bar UI
    }

    // Function to increase HP
    public void Heal(float healAmount)
    {
        currentHP += healAmount; // Increase current HP by healAmount
        if (currentHP > maxHP)
        {
            currentHP = maxHP; // Ensure HP doesn't exceed maxHP
        }
        UpdateHPBar(); // Update HP bar UI
    }

    // Function to update HP bar UI
    void UpdateHPBar()
    {
        float fillAmount = currentHP / maxHP; // Calculate fill amount based on current HP
        hpBarImage.fillAmount = fillAmount; // Update fill amount of the HP bar image
    }
}