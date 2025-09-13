using UnityEngine;

[AddComponentMenu("VPSV/Enemy Touch Damage 2D")]
public class EnemyTouchDamage2D : MonoBehaviour
{
    public int damage = 1;
    public float cooldown = 0.5f;     
    public LayerMask targetMask;      
    float _nextTime;

    void OnTriggerEnter2D(Collider2D other) { TryHit(other.gameObject); }
    void OnTriggerStay2D(Collider2D other) { TryHit(other.gameObject); }
    void OnCollisionEnter2D(Collision2D col) { TryHit(col.collider.gameObject); }
    void OnCollisionStay2D(Collision2D col) { TryHit(col.collider.gameObject); }

    void TryHit(GameObject go)
    {
        if (Time.time < _nextTime) return;
        if (targetMask.value != 0 && ((1 << go.layer) & targetMask.value) == 0) return;
        var ph = go.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            _nextTime = Time.time + cooldown;
            return;
        }
        var dmg = go.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            _nextTime = Time.time + cooldown;
        }
    }
}
