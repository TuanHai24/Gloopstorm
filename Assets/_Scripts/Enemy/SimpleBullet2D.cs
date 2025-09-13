using UnityEngine;

// Đạn 2D mượt: ưu tiên Rigidbody2D (có Interpolate), canh hướng theo vận tốc, optional tăng tốc nhẹ lúc đầu.
[AddComponentMenu("VPSV/SimpleBullet2D")]
public class SimpleBullet2D : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 6f;
    public float lifeTime = 2.5f;
    public float rampUpTime = 0f;       // 0 = phóng ngay; 0.05–0.1s = mượt hơn khi xuất hiện
    public bool alignRotation = true;   // quay sprite theo hướng bay
    public float rotateOffset = 0f;     // sprite hướng ↑ thì để -90; hướng → để 0

    [Header("Hit")]
    public float damage = 5f;
    public LayerMask hitMask;           // để Player cho đạn enemy
    public bool destroyOnHit = true;

    Rigidbody2D _rb;
    Vector2 _dir = Vector2.right;
    float _age = 0f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            // auto cấu hình cho mượt & ổn định
            _rb.gravityScale = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    // Khởi động đạn
    public void Init(Vector2 dir, float speedOverride = -1f, float lifeOverride = -1f)
    {
        _dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        if (speedOverride > 0f) speed = speedOverride;
        if (lifeOverride > 0f) lifeTime = lifeOverride;

        _age = 0f;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;  // nếu có rampUp sẽ tăng dần
        UpdateRotation();
        CancelInvoke(nameof(SelfDestruct));
        Invoke(nameof(SelfDestruct), lifeTime);
    }

    void Update()
    {
        _age += Time.deltaTime;
        float k = (rampUpTime > 0f) ? Mathf.Clamp01(_age / rampUpTime) : 1f;
        float v = speed * k;

        if (_rb != null)
        {
            // di chuyển bằng Rigidbody cho mượt (có Interpolate)
            _rb.linearVelocity = _dir * v;
        }
        else
        {
            // nếu không có RB thì tự di chuyển
            transform.position += (Vector3)(_dir * v * Time.deltaTime);
        }

        if (alignRotation) UpdateRotation();
    }

    void UpdateRotation()
    {
        var dir = (_rb != null) ? _rb.linearVelocity : _dir;
        if (dir.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotateOffset;
            transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hitMask.value != 0 && ((1 << other.gameObject.layer) & hitMask.value) == 0) return;

        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null) dmg.TakeDamage(damage);
        if (destroyOnHit) Destroy(gameObject);
    }

    void SelfDestruct() { Destroy(gameObject); }
}

// Interface nhẹ để nhận sát thương
public interface IDamageable { void TakeDamage(float amount); }
