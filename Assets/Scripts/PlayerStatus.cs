using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour, LivingBeing
{
    [Header("Health Settings")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    [Header("Inventory")]
    public bool hasSword = false;
    public bool hasRockSpell = false;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log($"Player took {amount} damage. Current HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died. Restarting level...");
        StartCoroutine(RestartAfterDelay(2f));
    }

    private System.Collections.IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GiveItem(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "sword":
                hasSword = true;
                break;
            case "rockspell":
                hasRockSpell = true;
                break;
            default:
                Debug.LogWarning($"Unknown item: {itemName}");
                break;
        }
    }

    public bool HasItem(string itemName)
    {
        return itemName.ToLower() switch
        {
            "sword" => hasSword,
            "rockspell" => hasRockSpell,
            _ => false
        };
    }
}
