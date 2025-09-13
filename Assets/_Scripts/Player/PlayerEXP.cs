using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEXP : MonoBehaviour
{
    public int level = 1;
    public int currentExp = 0;
    public int expNeeded = 3;

    [Header("UI")]
    public Slider expSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expNumbersText;   // NEW: "10/20" ở giữa đáy màn hình
    public LevelUpUI levelUpUI;

    void Start() => UpdateUI();

    public void AddExp(int amount)
    {
        currentExp += amount;

        // Có thể lên nhiều cấp trong 1 lần
        while (currentExp >= expNeeded)
        {
            currentExp -= expNeeded;
            level++;
            expNeeded = NextExpNeeded(level, expNeeded);

            if (levelUpUI != null)
                levelUpUI.RequestPick();   // mỗi cấp 1 lượt chọn
        }

        UpdateUI();
    }

    int NextExpNeeded(int newLevel, int prevNeed)
    {
        // tuỳ ý curve
        return Mathf.RoundToInt(prevNeed * 1.25f + 1);
    }

    void UpdateUI()
    {
        if (expSlider)
        {
            expSlider.maxValue = expNeeded;
            expSlider.value = Mathf.Clamp(currentExp, 0, expNeeded);
        }

        if (levelText)
            levelText.text = $"Lv {level}";

        if (expNumbersText)
            expNumbersText.text = $"{currentExp}/{expNeeded}";
    }
}
