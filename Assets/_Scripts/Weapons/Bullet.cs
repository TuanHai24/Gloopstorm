using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 1;
    public int pierce = 0;              // 0 = trúng 1 mục tiêu là huỷ
    public float lifeTime = 3f;

    Rigidbody2D rb;
    HashSet<EnemyHealth> hitOnce = new HashSet<EnemyHealth>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (lifeTime > 0) Destroy(gameObject, lifeTime);
    }

    // GỌI NGAY SAU KHI Instantiate
    public void Init(Vector2 dir, float speed)
    {
        rb.linearVelocity = dir.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<EnemyHealth>(out var hp)) return;

        // tránh dính đòn nhiều lần vì overlap
        if (hitOnce.Contains(hp)) return;
        hitOnce.Add(hp);

        hp.TakeDamage(damage);

        if (pierce <= 0) Destroy(gameObject);
        else pierce--;
    }
}
