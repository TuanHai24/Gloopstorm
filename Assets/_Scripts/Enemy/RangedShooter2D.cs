using UnityEngine;

[AddComponentMenu("VPSV/RangedShooter2D")]
public class RangedShooter2D : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;      // kéo DeathBullet vào
    public float bulletSpeed = 8f;
    public float bulletLifetime = 2.5f;
    public float spawnOffset = 0.20f;    // lùi khỏi thân để không cấn collider
    public float spriteAngleOffset = 0f; // sprite đạn hướng ↑ thì để -90, hướng → để 0

    [Header("Timing")]
    public float interval = 1.25f;       // bắn mỗi X giây
    public Vector2 intervalJitter = new Vector2(-0.2f, 0.3f); // ngẫu nhiên cho tự nhiên

    [Header("Range Gate")]
    public float minShootDistance = 2f;  // quá gần thì không bắn (tránh bắn xẹt qua người)
    public float maxShootDistance = 12f; // quá xa thì không bắn

    float _timer;
    Transform _target;

    void OnEnable()
    {
        _timer = interval * Random.Range(0.5f, 1f);
        _target = WaveManager.Instance ? WaveManager.Instance.player
                 : GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!_target || bulletPrefab == null) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            float dist = Vector2.Distance(transform.position, _target.position);
            if (dist >= minShootDistance && dist <= maxShootDistance)
                ShootAt(_target.position);

            _timer = interval + Random.Range(intervalJitter.x, intervalJitter.y);
        }
    }

    void ShootAt(Vector3 worldPos)
    {
        Vector2 dir = ((Vector2)worldPos - (Vector2)transform.position).normalized;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector3 pos = transform.position + (Vector3)(dir * spawnOffset);
        var proj = Instantiate(bulletPrefab, pos, Quaternion.AngleAxis(ang + spriteAngleOffset, Vector3.forward));

        var sb = proj.GetComponent<SimpleBullet2D>();
        if (sb == null) sb = proj.AddComponent<SimpleBullet2D>();
        sb.Init(dir, bulletSpeed, bulletLifetime);
    }
}
