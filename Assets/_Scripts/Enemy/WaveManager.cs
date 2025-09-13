using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EnemyWeight { public EnemyType type; public float weight = 1f; }

[System.Serializable]
public class WavePhase
{
    [Min(5f)] public float duration = 60f;
    [Min(0f)] public float budgetPerSecond = 4f;
    [Min(0)] public int targetAlive = 12;
    public List<EnemyWeight> pool = new List<EnemyWeight>();

    // one-shot (Boss)
    public bool spawnOnce = false;
    public GameObject spawnOncePrefab;
    [Min(0f)] public float spawnAt = 0f;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Phases")] public List<WavePhase> phases = new();
    [Header("Refs")] public Camera cam; public Transform player; public Transform enemiesParent; public LayerMask obstacleMask;
    [Header("Spawn")] public float spawnMargin = 2.0f; public float minSpawnDistToPlayer = 3.5f;
    [Header("End")] public bool pauseTimeOnEnd = false; public UnityEvent onGameEnd;

    int _phaseIndex = 0; float _phaseTimer = 0f; float _budget = 0f; int _alive = 0;
    bool _didSpawnOnce = false, _gameEnded = false;

    void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; if (cam == null) cam = Camera.main; }

    void Update()
    {
        if (_gameEnded || _phaseIndex >= phases.Count) return;
        var ph = phases[_phaseIndex];

        // Boss one-shot
        if (ph.spawnOnce && !_didSpawnOnce && _phaseTimer >= ph.spawnAt)
            if (ph.spawnOncePrefab != null && TrySpawn(ph.spawnOncePrefab)) { _didSpawnOnce = true; _alive++; }

        // budget + giữ targetAlive
        _budget += ph.budgetPerSecond * Time.deltaTime;
        int cheapest = GetCheapestCost(ph.pool);
        int guard = 20;
        while (guard-- > 0 && _alive < ph.targetAlive && _budget >= cheapest)
        {
            var choice = PickEnemyType(ph.pool);
            if (choice == null || choice.prefab == null) break;
            if (TrySpawn(choice.prefab)) { _budget -= Mathf.Max(1, choice.cost); _alive++; } else break;
        }

        // phase timing
        _phaseTimer += Time.deltaTime;
        if (_phaseTimer >= ph.duration) { _phaseIndex++; _phaseTimer = 0f; _budget = 0f; _didSpawnOnce = false; }
    }

    // API: gọi từ EnemyHealth
    public void ReportEnemyDied(bool isBoss = false)
    {
        _alive = Mathf.Max(0, _alive - 1);
        if (isBoss && !_gameEnded) EndGame();
    }

    void EndGame()
    {
        _gameEnded = true;
        if (pauseTimeOnEnd) Time.timeScale = 0f;
        onGameEnd?.Invoke();
        Debug.Log("[WaveManager] GAME END");
    }

    // ----- internals -----
    int GetCheapestCost(List<EnemyWeight> pool)
    {
        int cheapest = int.MaxValue;
        foreach (var e in pool) { if (e == null || e.type == null) continue; cheapest = Mathf.Min(cheapest, Mathf.Max(1, e.type.cost)); }
        return cheapest == int.MaxValue ? 9999 : cheapest;
    }

    EnemyType PickEnemyType(List<EnemyWeight> pool)
    {
        float total = 0f; foreach (var e in pool) if (e != null && e.type != null) total += Mathf.Max(0f, e.weight);
        if (total <= 0f) return null;
        float r = Random.value * total, acc = 0f;
        foreach (var e in pool) { if (e == null || e.type == null) continue; acc += Mathf.Max(0f, e.weight); if (r <= acc) return e.type; }
        return pool[pool.Count - 1].type;
    }

    bool TrySpawn(GameObject prefab)
    {
        if (prefab == null || cam == null) return false;
        if (!FindSpawnPos(out Vector2 pos)) return false;
        var go = Instantiate(prefab, pos, Quaternion.identity, enemiesParent);
        return go != null;
    }

    bool FindSpawnPos(out Vector2 pos)
    {
        var fr = CamWorldRect(); var center = fr.center;
        float halfDiag = Mathf.Sqrt(fr.width * fr.width + fr.height * fr.height) * 0.5f;
        for (int i = 0; i < 24; i++)
        {
            float ang = Random.value * Mathf.PI * 2f;
            float dist = halfDiag + spawnMargin + Random.Range(0f, 1.5f);
            Vector2 p = (Vector2)center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist;
            if (player != null && Vector2.Distance(p, player.position) < minSpawnDistToPlayer) continue;
            if (obstacleMask.value != 0)
            {
                var dir = (center - p).normalized;
                if (Physics2D.Raycast(p, dir, dist, obstacleMask)) continue;
            }
            pos = p; return true;
        }
        pos = Vector2.zero; return false;
    }

    Rect CamWorldRect()
    {
        float h = 2f * cam.orthographicSize; float w = h * cam.aspect; Vector3 c = cam.transform.position;
        return new Rect(c.x - w / 2f, c.y - h / 2f, w, h);
    }

#if UNITY_EDITOR
    [ContextMenu("Fill Sample Phases (with Boss)")]
    void FillSamplePhases()
    {
        phases = new List<WavePhase>{
            new WavePhase{ duration=60f, budgetPerSecond=3f,  targetAlive=10 },
            new WavePhase{ duration=80f, budgetPerSecond=4.5f, targetAlive=14 },
            new WavePhase{ duration=20f, budgetPerSecond=1.2f, targetAlive=8 },
            new WavePhase{ duration=30f, budgetPerSecond=0f,   targetAlive=0, spawnOnce=true, spawnAt=0.5f }
        };
        Debug.Log("Sample phases ready. Add Slime/Ranged to pools; Phase 4 -> spawnOncePrefab = Boss.");
    }
#endif
}
