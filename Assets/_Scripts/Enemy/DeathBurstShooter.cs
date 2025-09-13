using UnityEngine;

public enum DeathBurstTriggerMode { ManualCall, OnDestroy, OnDisable }
public enum BurstMode { Radial, AimAtPlayer }   // NEW

[AddComponentMenu("VPSV/DeathBurstShooter")]
public class DeathBurstShooter : MonoBehaviour
{
    [Header("Trigger")]
    public DeathBurstTriggerMode triggerMode = DeathBurstTriggerMode.ManualCall;

    [Header("Mode")]
    public BurstMode mode = BurstMode.Radial;   // NEW: chọn Radial hay AimAtPlayer

    [Header("Burst Config")]
    public GameObject bulletPrefab;
    [Min(1)] public int bulletCount = 8;
    [Range(0f, 360f)] public float angleSpread = 360f; // dùng cho Radial
    public float angleJitter = 10f;                     // nhiễu nhỏ để tự nhiên
    public float bulletSpeed = 6f;
    public float bulletLifetime = 2.5f;

    [Header("AimAtPlayer Settings")]
    public float aimedSpread = 10f;       // tổng độ rộng chùm (ví dụ 10° cho 1–3 viên)
    public float spawnOffset = 0.20f;     // lùi ra khỏi enemy để khỏi cấn collider
    public float spriteAngleOffset = 0f;  // sprite của m quay lên trên? set = -90

    [Header("Reporting (optional; mặc định dùng EnemyHealth)")]
    public bool reportToWaveManager = false;
    public bool isBoss = false;

    private bool _hasBurst = false;

    public void TriggerBurst()
    {
        if (_hasBurst) return;
        _hasBurst = true;

        if (bulletPrefab == null)
        {
            Debug.LogWarning("[DeathBurstShooter] bulletPrefab is null.");
        }
        else
        {
            if (mode == BurstMode.AimAtPlayer)
                FireAimed();
            else
                FireRadial();
        }

        if (reportToWaveManager && WaveManager.Instance != null)
            WaveManager.Instance.ReportEnemyDied(isBoss);
    }

    // ----- Modes -----
    void FireRadial()
    {
        float start = Random.Range(-angleJitter, angleJitter);
        int n = Mathf.Max(1, bulletCount);
        float step = (n > 1) ? (angleSpread / n) : 0f;

        for (int i = 0; i < n; i++)
        {
            float a = start + step * i;
            SpawnBullet(dirFromAngle(a), a);
        }
    }

    void FireAimed()
    {
        // Tìm player
        Transform t = null;
        if (WaveManager.Instance != null && WaveManager.Instance.player != null)
            t = WaveManager.Instance.player;
        else
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) t = go.transform;
        }

        Vector2 baseDir = Vector2.right;
        if (t != null) baseDir = ((Vector2)t.position - (Vector2)transform.position).normalized;

        float baseAng = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        int n = Mathf.Max(1, bulletCount);
        float spread = Mathf.Max(0f, aimedSpread);
        float start = baseAng - spread * 0.5f;
        float step = (n > 1) ? (spread / (n - 1)) : 0f;

        for (int i = 0; i < n; i++)
        {
            float a = start + step * i + Random.Range(-angleJitter, angleJitter);
            SpawnBullet(dirFromAngle(a), a);
        }
    }

    void SpawnBullet(Vector2 dir, float angleDeg)
    {
        Vector3 pos = transform.position + (Vector3)(dir * spawnOffset);
        Quaternion rot = Quaternion.AngleAxis(angleDeg + spriteAngleOffset, Vector3.forward);

        GameObject proj = Instantiate(bulletPrefab, pos, rot);
        var sb = proj.GetComponent<SimpleBullet2D>();
        if (sb == null) sb = proj.AddComponent<SimpleBullet2D>();
        sb.Init(dir, bulletSpeed, bulletLifetime);
    }

    static Vector2 dirFromAngle(float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    private void OnDestroy()
    {
        if (triggerMode == DeathBurstTriggerMode.OnDestroy && !_hasBurst)
            TriggerBurst();
    }
    private void OnDisable()
    {
        if (triggerMode == DeathBurstTriggerMode.OnDisable && !_hasBurst)
            TriggerBurst();
    }
}
