using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Delegate for health changes
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    //Changes
    public delegate void OnDamageTakenDelegate();
    public OnDamageTakenDelegate onDamageTaken;

    public delegate void OnDeathDelegate();
    public OnDeathDelegate onDeath;

    public ParticleSystem starsEffect;

    #region Singleton
    private static PlayerStats instance;
    public static PlayerStats Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PlayerStats>();
            return instance;
        }
    }
    #endregion

    [Header("Health Settings")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxTotalHealth;

    [Header("References")]
    [SerializeField] private GameManagerScript gameManager;

    private bool isDead = false;

    // Public read-only properties
    public float Health => health;
    public float MaxHealth => maxHealth;
    public float MaxTotalHealth => maxTotalHealth;
    public GameManagerScript GameManager => gameManager;

    // Heal the player
    public void Heal(float amount)
    {
        health += amount;
        ClampHealth();
    }

    // Take damage
    /*public void TakeDamage(float dmg)
    {
        health -= dmg;
        ClampHealth();

        if (health <= 0 && !isDead)
        {
            isDead = true;
            if (gameManager != null)
                gameManager.gameOver();
            Debug.Log("Player is dead");
        }
    }*/
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        ClampHealth();

        onDamageTaken?.Invoke();

        if (health <= 0 && !isDead)
        {
            isDead = true;
            onDeath?.Invoke();

            if (gameManager != null)
                gameManager.gameOver();

            Debug.Log("Player is dead");
        }
    }

    // Increase max health
    public void AddHealth()
    {
        if (maxHealth < maxTotalHealth)
        {
            maxHealth += 1;
            health = maxHealth;

            onHealthChangedCallback?.Invoke();
        }
    }

    // Ensure health is within valid bounds
    private void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        onHealthChangedCallback?.Invoke();
    }
}


