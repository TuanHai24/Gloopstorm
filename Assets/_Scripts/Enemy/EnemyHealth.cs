using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("VPSV/EnemyHealth")]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public float maxHP = 20f;
    public bool isBoss = false;

    [Header("Death (timing & effects)")]
    public DeathBurstShooter deathBurst;     
    public UnityEvent onDeath;               

    [Tooltip("Nếu true, không biến mất ngay mà đợi một khoảng để xem anim/VFX.")]
    public bool deferDestroy = true;

    [Min(0f)]
    public float deferDestroySeconds = 0.4f; 

    [Tooltip("Nếu true: Destroy gameObject khi hết defer; nếu false: SetActive(false) (hợp pooling).")]
    public bool destroyOnDeath = true;

    [Header("Optional auto-disable khi chết")]
    public bool disableAllCollidersOnDeath = true;
    public bool disableRigidbody2DOnDeath = true;

    float _hp;

    void OnEnable() { _hp = maxHP; }

    public void TakeDamage(float amount)
    {
        if (_hp <= 0f) return;
        _hp -= amount;
        if (_hp <= 0f) Die();
    }

    public void Die()
    {
        if (deathBurst == null) deathBurst = GetComponent<DeathBurstShooter>();
        if (deathBurst != null)
        {
            deathBurst.triggerMode = DeathBurstTriggerMode.ManualCall;
            deathBurst.reportToWaveManager = false;
            deathBurst.TriggerBurst();
        }

        onDeath?.Invoke();

        if (disableAllCollidersOnDeath)
        {
            var cols = GetComponents<Collider2D>();
            for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;
        }
        if (disableRigidbody2DOnDeath)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb) rb.simulated = false;
        }

        if (WaveManager.Instance != null)
            WaveManager.Instance.ReportEnemyDied(isBoss);

        if (deferDestroy)
        {
            if (destroyOnDeath) Destroy(gameObject, deferDestroySeconds);
            else Invoke(nameof(DoDisable), deferDestroySeconds);
        }
        else
        {
            if (destroyOnDeath) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }

    void DoDisable() => gameObject.SetActive(false);
}
