using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public enum FireMode { Fan, Radial, Spiral } // Spiral ở đây nếu m CHỌN làm mode chính

    [Header("References")]
    public GameObject bulletPrefab;

    [Header("Stats")]
    public int damage = 5;
    public float attackCooldown = 1f;
    public float bulletSpeed = 10f;
    public int bulletPierce = 0;

    [Header("Multi / Fan / Chain")]
    public int bulletCount = 1;
    public int bulletChain = 0;
    public float chainRadius = 2f;

    [Header("Fire Modes (base)")]
    public FireMode fireMode = FireMode.Fan;
    public float spiralRPM = 120f;      // dùng khi CHỌN Spiral làm mode chính
    public int radialBullets = 16;
    public float radialCooldown = 0.7f;

    [Header("Evo – Spiral (pattern PHỤ)")]
    public bool evoSpiral = false;
    public float evoSpiralRPM = 240f;
    public float evoSpiralCooldown = 0.06f;

    [Header("Evo – Radial (pattern PHỤ)")]
    public bool evoRadial = false;
    public int evoRadialBullets = 12;
    public float evoRadialCooldown = 0.5f;

    [Header("Spawn")]
    [Tooltip("Đẩy chỗ spawn ra khỏi người (m). Sẽ auto theo collider nếu lớn hơn.")]
    public float spawnOffset = 0.6f;

    float timer;
    float spiralTimer;
    float evoRadialTimer;
    float spiralAngle; // độ

    Rigidbody2D rb;
    Collider2D[] ownerCols;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ownerCols = GetComponentsInChildren<Collider2D>();

        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            float guess = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
            spawnOffset = Mathf.Max(spawnOffset, guess * 0.7f);
        }
    }

    void Start()
    {
        Debug.Log($"[PlayerAttack] start. bulletCount={bulletCount}");
    }

    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        // Pattern phụ – không thay đổi fireMode gốc
        if (evoSpiral) HandleEvoSpiral(dt);
        if (evoRadial) HandleEvoRadial(dt);

        switch (fireMode)
        {
            case FireMode.Fan:
                if (timer >= attackCooldown)
                {
                    timer = 0f;
                    FireFan();
                }
                break;

            case FireMode.Radial:
                if (timer >= radialCooldown)
                {
                    timer = 0f;
                    FireRadial(radialBullets);
                }
                break;

            case FireMode.Spiral:
                // Spiral làm mode CHÍNH (khác với evoSpiral phụ)
                float perShot = Mathf.Max(0.02f, 60f / spiralRPM); // giây/viên
                if (timer >= perShot)
                {
                    timer = 0f;
                    spiralAngle += spiralRPM * 6f * perShot; // RPM->deg/s
                    float rad = spiralAngle * Mathf.Deg2Rad;
                    FireDir(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
                }
                break;
        }
    }

    // ---------- Patterns ----------
    void FireFan()
    {
        Vector2 baseDir = Vector2.right;
        var t = FindNearestEnemy();
        if (t != null) baseDir = ((Vector2)t.position - (Vector2)transform.position).normalized;

        int n = Mathf.Max(1, bulletCount);
        if (n == 1) { FireDir(baseDir); return; }

        float totalSpread = Mathf.Clamp(10f * (n - 1), 0f, 60f);
        for (int i = 0; i < n; i++)
        {
            float a = -totalSpread * 0.5f + totalSpread * (i / (n - 1f));
            Vector2 d = Rotate(baseDir, a);
            FireDir(d);
        }
    }

    void FireRadial(int count)
    {
        int n = Mathf.Max(1, count);
        float step = 360f / n;
        for (int i = 0; i < n; i++)
        {
            float a = step * i;
            Vector2 d = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
            FireDir(d);
        }
    }

    void HandleEvoSpiral(float dt)
    {
        spiralAngle += evoSpiralRPM * 6f * dt;
        spiralTimer += dt;
        if (spiralTimer >= evoSpiralCooldown)
        {
            spiralTimer = 0f;
            float rad = spiralAngle * Mathf.Deg2Rad;
            Vector2 d = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            FireDir(d);
        }
    }

    void HandleEvoRadial(float dt)
    {
        evoRadialTimer += dt;
        if (evoRadialTimer >= evoRadialCooldown)
        {
            evoRadialTimer = 0f;
            FireRadial(evoRadialBullets);
        }
    }

    // ---------- Core shot ----------
    void FireDir(Vector2 dir)
    {
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;

        var go = Instantiate(bulletPrefab, spawnPos, Quaternion.FromToRotation(Vector2.right, dir));

        int bulletLayer = LayerMask.NameToLayer("PlayerBullet");
        if (bulletLayer >= 0) go.layer = bulletLayer;

        var b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.damage = damage;            // nếu muốn đạn lấy sát thương từ PlayerAttack
            b.pierce = bulletPierce;      // số mục tiêu xuyên qua
            b.Init(dir, bulletSpeed);     // CHỈ 2 tham số: hướng + tốc độ
        }
    }


    // ---------- Helpers ----------
    Transform FindNearestEnemy()
    {
        GameObject[] list = GameObject.FindGameObjectsWithTag("Enemy");
        float best = float.PositiveInfinity;
        Transform res = null;
        Vector2 p = transform.position;

        for (int i = 0; i < list.Length; i++)
        {
            var tr = list[i].transform;
            float d = ((Vector2)tr.position - p).sqrMagnitude;
            if (d < best) { best = d; res = tr; }
        }
        return res;
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad);
        float sa = Mathf.Sin(rad);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }
}
