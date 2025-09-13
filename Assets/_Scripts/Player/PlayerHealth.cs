using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Player nhận damage từ SimpleBullet2D (gọi IDamageable.TakeDamage(float))
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public int maxHP = 10;
    int currentHP;

    [Header("Regen")]
    public float regenPerSec = 0f;
    float regenAcc = 0f;

    [Header("HUD (tuỳ chọn)")]
    public Slider healthSlider;            
    public TextMeshProUGUI hpText;         

    [Header("World-space HP (đang dùng)")]
    public TextMeshProUGUI hpWorldText;    
    public HealthBarWorld worldHP;         

    void Start()
    {
        currentHP = maxHP;
        if (worldHP == null) worldHP = GetComponentInChildren<HealthBarWorld>();
        if (hpWorldText == null && worldHP != null)
            hpWorldText = worldHP.GetComponentInChildren<TextMeshProUGUI>();

        RefreshHPUI();
    }

    void Update()
    {
        if (regenPerSec > 0f && currentHP > 0 && currentHP < maxHP)
        {
            regenAcc += regenPerSec * Time.deltaTime;
            if (regenAcc >= 1f)
            {
                int heal = Mathf.FloorToInt(regenAcc);
                regenAcc -= heal;
                Heal(heal);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(Mathf.CeilToInt(amount));
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHP <= 0) return;
        currentHP = Mathf.Max(0, currentHP - damage);
        RefreshHPUI();
        if (currentHP <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHP <= 0) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        RefreshHPUI();
    }

    public void SetMaxHP(int newMax, bool healToFull = false)
    {
        maxHP = Mathf.Max(1, newMax);
        if (healToFull) currentHP = maxHP;
        currentHP = Mathf.Min(currentHP, maxHP);
        RefreshHPUI();
    }

    void RefreshHPUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }

        string s = $"{currentHP}/{maxHP}";
        if (hpText != null) hpText.text = s;
        if (hpWorldText != null) hpWorldText.text = s;

        if (worldHP != null)
        {
            float pct = (maxHP > 0) ? (float)currentHP / maxHP : 0f;
            worldHP.Set(pct);
        }
    }

    void Die()
    {
        FindObjectOfType<GameOverUI>()?.Show();
        Time.timeScale = 0f;
    }

    public int CurrentHP() => currentHP;
}
