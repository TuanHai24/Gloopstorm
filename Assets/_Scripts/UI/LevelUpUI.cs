using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    [Header("Refs")]
    public GameObject panel;
    public UpgradeRegistry registry;
    public UpgradeManager upgradeManager;

    [Header("3 lựa chọn")]
    public Button[] optionButtons;
    public Image[] optionIcons;
    public TextMeshProUGUI[] optionTitles;
    public TextMeshProUGUI[] optionDescs;

    [Header("Special")]
    public UpgradeData rerollSO;
    public UpgradeData skipSO;

    [Header("Evolve gating")]
    [Tooltip("Prereq chỉ có thể xuất hiện nếu base weapon đạt >= (requiredLevel - leadWindow).")]
    public int prereqLeadWindow = 2;   // ví dụ: cho xuất hiện sớm 1-2 cấp trước mốc evolve

    // pick queue (nhiều cấp một lúc)
    public int pendingPicks = 0;

    private UpgradeData[] current = new UpgradeData[3];

    void Awake()
    {
        if (panel) panel.SetActive(false);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i;
            optionButtons[i].onClick.AddListener(() => Pick(idx));
        }

        // (khuyến nghị) cấu hình TMP cho khỏi cắt chữ
        if (optionTitles != null)
            foreach (var t in optionTitles) if (t) { t.enableAutoSizing = true; t.fontSizeMin = 18; t.fontSizeMax = 34; t.enableWordWrapping = true; t.overflowMode = TextOverflowModes.Overflow; t.alignment = TextAlignmentOptions.Center; }
        if (optionDescs != null)
            foreach (var t in optionDescs) if (t) { t.enableAutoSizing = false; t.overflowMode = TextOverflowModes.Truncate; t.alignment = TextAlignmentOptions.Center; }
    }

    // Gọi mỗi lần lên 1 cấp
    public void RequestPick()
    {
        pendingPicks++;
        if (!panel.activeSelf) Show();
    }

    public void Show()
    {
        BuildOptions();
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Hide()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ----------- Build 3 lựa chọn -----------
    void BuildOptions()
    {
        var pool = new List<UpgradeData>();

        foreach (var u in registry.upgrades)
        {
            if (u == null) continue;

            // SPECIAL xử lý riêng
            if (u.kind == UpgradeKind.Special) continue;

            // PrereqOnly: chỉ add vào pool nếu đủ điều kiện xuất hiện
            if (u.kind == UpgradeKind.PrereqOnly || u.isPrereqOnly)
            {
                if (ShouldOfferPrereq(u)) pool.Add(u);
                continue;
            }

            // Upgrade thường: nếu chưa max thì add
            bool isMaxed = (u.maxLevel > 0) && (upgradeManager.GetLevel(u.id) >= u.maxLevel);
            if (!isMaxed) pool.Add(u);
        }

        // Weighted = pool + Special (nếu có)
        var weighted = new List<UpgradeData>(pool);
        if (rerollSO) weighted.Add(rerollSO);
        if (skipSO) weighted.Add(skipSO);

        var picked = PickDistinct(weighted, 3);

        // Render
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool has = i < picked.Count;
            optionButtons[i].gameObject.SetActive(has);
            optionIcons[i].gameObject.SetActive(has);
            optionTitles[i].gameObject.SetActive(has);
            optionDescs[i].gameObject.SetActive(has);
            if (!has) continue;

            current[i] = picked[i];
            var u = picked[i];

            bool obscure = ShouldObscure(u);

            optionIcons[i].sprite = obscure && u.unknownIcon ? u.unknownIcon : u.icon;
            optionTitles[i].text = obscure ? (string.IsNullOrWhiteSpace(u.unknownName) ? "???" : u.unknownName)
                                            : u.displayName;
            optionDescs[i].text = BuildDesc(u, obscure);
        }
    }

    List<UpgradeData> PickDistinct(List<UpgradeData> source, int count)
    {
        var result = new List<UpgradeData>();
        if (source == null || source.Count == 0) return result;

        var temp = new List<UpgradeData>(source);
        int n = Mathf.Min(count, temp.Count);
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(0, temp.Count);
            result.Add(temp[r]);
            temp.RemoveAt(r);
        }
        return result;
    }

    // Chỉ hiện prereq khi thực sự có ý nghĩa cho evolve
    bool ShouldOfferPrereq(UpgradeData prereq)
    {
        // 1) Đã có rồi -> không hiện nữa
        if (upgradeManager.HasPrereq(prereq.id)) return false;

        // 2) Tìm base weapon nào cần prereq này và đang gần đủ cấp evolve
        foreach (var baseW in registry.upgrades)
        {
            if (baseW == null || !baseW.canEvolve) continue;
            if (baseW.requiredPrereqId != prereq.id) continue;

            int curLv = upgradeManager.GetLevel(baseW.id);
            // baseW gần mốc evolve và chưa max (đề phòng vũ khí đã max)
            bool nearLevel = curLv >= Mathf.Max(0, baseW.requiredLevel - prereqLeadWindow);
            bool notMaxed = (baseW.maxLevel <= 0) || (curLv < baseW.maxLevel);

            if (nearLevel && notMaxed)
                return true; // cho vào pool (có tỉ lệ xuất hiện)
        }
        return false;
    }

    bool ShouldObscure(UpgradeData u)
    {
        if (u == null) return false;
        if (!u.obscure) return false;
        if (u.kind == UpgradeKind.Special) return false;
        if (u.kind == UpgradeKind.PrereqOnly) return false;
        if (upgradeManager.GetLevel(u.id) >= u.maxLevel) return false;
        if (UpgradeDiscovery.IsDiscovered(u.id)) return false;
        return true;
    }

    string BuildDesc(UpgradeData u, bool obscure)
    {
        if (u.kind == UpgradeKind.Special)
            return u.isReroll ? "Đổi 3 lựa chọn khác." : "Bỏ qua lần này.";

        if (u.kind == UpgradeKind.PrereqOnly || u.isPrereqOnly)
            return obscure ? (string.IsNullOrWhiteSpace(u.unknownDesc) ? "Có vẻ… quan trọng." : u.unknownDesc)
                           : (string.IsNullOrWhiteSpace(u.descTemplate) ? "Điều kiện tiến hoá." : u.descTemplate);

        int curLv = upgradeManager.GetLevel(u.id);
        int nextIdx = Mathf.Clamp(curLv, 0, (u.values != null && u.values.Length > 0) ? u.values.Length - 1 : 0);
        float nextV = (u.values != null && u.values.Length > 0) ? u.values[nextIdx] : 0f;

        if (obscure)
        {
            return string.IsNullOrWhiteSpace(u.unknownDesc) ? "Tăng hiệu quả một cách đáng kể." : u.unknownDesc;
        }

        string body = "";
        if (!string.IsNullOrWhiteSpace(u.descTemplate))
        {
            string fmt = string.IsNullOrEmpty(u.valueFormat) ? "0.#" : u.valueFormat;
            string vStr = u.hideNumbers ? "một chút" : nextV.ToString(fmt);
            body = u.descTemplate.Replace("{V}", vStr);
        }

        string footer = $"<size=80%><color=#CCCCCC>Lv {curLv}/{u.maxLevel}</color></size>";
        string result = string.IsNullOrWhiteSpace(body) ? footer : (body + "\n" + footer);

        if (u.canEvolve && !u.hideEvoHint)
        {
            string preName = "";
            if (!string.IsNullOrEmpty(u.requiredPrereqId))
            {
                var so = registry.Get(u.requiredPrereqId);
                preName = so != null ? so.displayName : u.requiredPrereqId;
            }
            string need = $"Yêu cầu tiến hoá: Lv{u.requiredLevel}" + (string.IsNullOrEmpty(preName) ? "" : $" + {preName}");
            string to = (u.evolveToPattern != EvoPattern.None) ? $" → {u.evolveToPattern}" : "";
            result += $"\n<size=70%><color=#9A9A9A>{need}{to}</color></size>";
        }
        return result;
    }

    // ----------- Pick -----------
    void Pick(int idx)
    {
        var u = current[idx];
        if (u == null) return;

        if (u.kind == UpgradeKind.Special)
        {
            if (u.isReroll) { BuildOptions(); return; }
            if (u.isSkip) { pendingPicks = Mathf.Max(0, pendingPicks - 1); NextOrClose(); return; }
        }

        upgradeManager.ApplyUpgrade(u);
        pendingPicks = Mathf.Max(0, pendingPicks - 1);
        NextOrClose();
    }

    void NextOrClose()
    {
        if (pendingPicks > 0) BuildOptions();
        else Hide();
    }
}
