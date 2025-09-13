using System.Collections.Generic;
using UnityEngine;
using static PlayerAttack;

public class UpgradeManager : MonoBehaviour
{
    [Header("Registry")]
    public UpgradeRegistry registry;

    [Header("Gameplay Refs (kéo vào từ Player)")]
    public PlayerAttack playerAttack;
    public PlayerMovement playerMovement;
    public PlayerHealth playerHealth;
    public PickupMagnet pickupMagnet;   // NEW: để tăng bán kính hút EXP

    // Level của từng upgrade thường
    private readonly Dictionary<string, int> levels = new();

    // Các prereq đã sở hữu (chọn 1 lần là đủ)
    private readonly HashSet<string> ownedPrereqs = new();

    // --------- Query ----------
    public int GetLevel(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        return levels.TryGetValue(id, out var lv) ? lv : 0;
    }
    public bool HasPrereq(string prereqId)
    {
        return !string.IsNullOrEmpty(prereqId) && ownedPrereqs.Contains(prereqId);
    }

    // --------- Áp dụng nâng cấp ----------
    public void ApplyUpgrade(UpgradeData up)
    {
        if (up == null) return;

        // Thẻ điều kiện (PrereqOnly) chỉ đánh dấu là đã có
        if (up.kind == UpgradeKind.PrereqOnly || up.isPrereqOnly)
        {
            if (!ownedPrereqs.Contains(up.id))
                ownedPrereqs.Add(up.id);

            UpgradeDiscovery.MarkDiscovered(up.id); // để UI hết ẩn
            TryEvolveForPrereq(up.id);
            return;
        }

        // Upgrade thường → tăng level
        int cur = GetLevel(up.id);
        int newLevel = Mathf.Clamp(cur + 1, 1, up.maxLevel);
        levels[up.id] = newLevel;

        ApplyStats(up, newLevel);
        TryEvolve(up);
    }
    // Đã có GetLevel(id) rồi thì Has() chỉ đơn giản vậy:
    private bool Has(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return GetLevel(id) > 0;   // có level > 0 tức là đã lấy nâng cấp đó
    }

    // --------- Map chỉ số ----------
    private void ApplyStats(UpgradeData up, int newLevel)
    {

        int idx = Mathf.Clamp(newLevel - 1, 0,
            (up.values != null && up.values.Length > 0) ? up.values.Length - 1 : 0);
        float v = (up.values != null && up.values.Length > 0) ? up.values[idx] : 0f;

        switch (up.id)
        {
            case "multishot": playerAttack.bulletCount += Mathf.RoundToInt(v); break;
            case "damage_up": playerAttack.damage += Mathf.RoundToInt(v); break;
            case "firerate": playerAttack.attackCooldown = Mathf.Max(0.05f, playerAttack.attackCooldown - v); break;
            case "proj_speed": playerAttack.bulletSpeed += v; break;
            case "movespeed": playerMovement.moveSpeed += v; break;

            case "pierce": playerAttack.bulletPierce += Mathf.RoundToInt(v); break;  // NEW
            case "chain": playerAttack.bulletChain += Mathf.RoundToInt(v); break;
            case "chain_radius": playerAttack.chainRadius += v; break;

            case "maxhp": playerHealth.SetMaxHP(playerHealth.maxHP + Mathf.RoundToInt(v), false); break;
            case "regen": playerHealth.regenPerSec += v; break;

            case "magnet":
                {
                    var magnet = FindObjectOfType<PickupMagnet>();   // hoặc kéo reference qua Inspector
                    if (magnet != null) magnet.AddRadius(v);         // v lấy từ up.values[idx]
                    break;
                }


            default:
                Debug.LogWarning($"[Upgrade] id '{up.id}' chưa được map trong ApplyStats.");
                break;
        }
    }

    // --------- Evolve ----------
    // Trong UpgradeManager.cs – thay nguyên hàm TryEvolve bằng đoạn này
    void TryEvolve(UpgradeData baseWeapon)
    {
        if (baseWeapon == null || !baseWeapon.canEvolve) return;

        int enoughLevel = GetLevel(baseWeapon.id);
        bool hasPrereq = string.IsNullOrEmpty(baseWeapon.requiredPrereqId) || Has(baseWeapon.requiredPrereqId);
        if (!(enoughLevel >= baseWeapon.requiredLevel && hasPrereq)) return;

        switch (baseWeapon.evolveToPattern)
        {
            case EvoPattern.Spiral:
                playerAttack.evoSpiral = true;
                playerAttack.evoSpiralRPM = 240f;
                playerAttack.evoSpiralCooldown = 0.06f;
                break;

            case EvoPattern.Radial:
                playerAttack.evoRadial = true; // nếu sau này xài
                break;

            case EvoPattern.None:
            default:
                break;
        }
    }



    // Khi vừa nhặt 1 prereq, thử evolve tất cả base weapon cần nó
    private void TryEvolveForPrereq(string prereqId)
    {
        foreach (var u in registry.upgrades)
        {
            if (u == null || !u.canEvolve) continue;
            if (u.requiredPrereqId != prereqId) continue;
            TryEvolve(u);
        }
    }
}
