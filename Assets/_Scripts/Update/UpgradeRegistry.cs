using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/UpgradeRegistry")]
public class UpgradeRegistry : ScriptableObject
{
    public UpgradeData[] upgrades;

    Dictionary<string, UpgradeData> map;

    public void Init()
    {
        if (map != null) return;
        map = new Dictionary<string, UpgradeData>();
        if (upgrades == null) return;
        foreach (var u in upgrades)
        {
            if (u == null || string.IsNullOrEmpty(u.id)) continue;
            map[u.id] = u;
        }
    }

    public UpgradeData Get(string id)
    {
        Init();
        return (map != null && map.TryGetValue(id, out var u)) ? u : null;
    }
}
